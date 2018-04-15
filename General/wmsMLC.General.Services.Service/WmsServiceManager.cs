using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using wmsMLC.Business.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using wmsMLC.General.DAL.Oracle;
using wmsMLC.General.Services.Service.Telegrams;

namespace wmsMLC.General.Services.Service
{
    public class WmsServiceManager : IServiceManager
    {
        #region .  Fields  .
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IManagerForObject _managerForObject;

        protected readonly ILog Log;

        private static readonly ConcurrentDictionary<string, Lazy<MethodInfo>> MethodCache = new ConcurrentDictionary<string, Lazy<MethodInfo>>();
        #endregion

        public WmsServiceManager(IUnitOfWorkFactory uowFactory, IManagerForObject managerForObject)
        {
            Log = LogManager.GetLogger(GetType());
            _uowFactory = uowFactory;
            _managerForObject = managerForObject;
        }

        #region .  IServiceManager  .
        public Telegram ProcessTelegram(Telegram telegram)
        {
            Log.DebugFormat("Start processing telegram {0}", telegram.TransactionNumber);

            var startProcessingTime = DateTime.Now;
            var userSignature = telegram.UserInfo;
            var sessionId = telegram.FromId.ClientSessionId;
            var uowId = telegram.UnitOfWork;
            var timeOut = telegram.TimeOut;

            // если Guid.Empty, то мы имеем дело с короткой сессией (временем жизни управляем здесь же)
            bool disposeUow = uowId.Equals(Guid.Empty);

            // начинаем готовить ответную телеграмму
            var answer = new RepoAnswerTelegramWrapper(telegram);//  new Telegram { Content = new Node { Name = "ANS", Value = "RepoAnswer" } };

            //TODO: ПЕРЕПИСАТЬ ВСЕ с telegram НА RepoQueryWrapper

            // объявлем UnitOfWork (по сути - это сессия подключения, которая необходима нам для передачи подписи пользователя в БД + возможность организовать транзакцию)
            IUnitOfWork uow = null;
            try
            {
                if (telegram.Content.Nodes.Count == 0)
                {
                    Log.WarnFormat("Telegramm is without content. Ending processing");
                    return null;
                }

                var uowContext = new UnitOfWorkContext { UserSignature = userSignature, Id = uowId, SessionId = (int?)sessionId, TimeOut = timeOut };
                uow = _uowFactory.Create(uowContext);
                if (disposeUow)
                    Log.DebugFormat("Open short UnitOfWork session");
                else
                    Log.DebugFormat("Open long UnitOfWork session (id = {0})", uowId);

                // выставляем TimeOut - может быть изменен в процессе работы в транзакции
                if (timeOut.HasValue)
                    uow.TimeOut = timeOut;

                var requestedEntity = telegram.Content.Nodes[0].Name;
                var actionName = telegram.Content.Nodes[0].Value;
                var parameters = telegram.Content.Nodes[0].Nodes;

                // если пришла телеграмма транзакции
                if (requestedEntity.EqIgnoreCase(typeof(IUnitOfWork).Name))
                {
                    var uowMethodInfo = GetMethod(uow.GetType(), actionName, parameters);
                    if (uowMethodInfo == null)
                        throw new DeveloperException("Can't find method", new MissingMethodException(uow.GetType().Name, actionName));

                    // заполеяем коллекцию входных параметров метода
                    var uowMethodParams = uowMethodInfo.GetParameters();
                    var uowArgs = new object[uowMethodParams.Length];
                    for (var i = 0; i < uowMethodParams.Length; i++)
                        uowArgs[i] = GetParameterValue(uowMethodParams[i], parameters);

                    Log.DebugFormat("Invoke method {0}.{1}", uow.GetType().Name, uowMethodInfo.Name);
                    uowMethodInfo.Invoke(uow, uowArgs);

                    if (uowMethodInfo.Name.Equals("Dispose"))
                        Log.DebugFormat("Dispose long UnitOfWork session (id = {0})", uowId);

                    // Упаковываем секцию statictics
                    // TODO: уйти от этого
                    answer.AddStatisticNode("0");
                    return null;
                }

                var mgrType = _managerForObject.GetManagerByTypeName(requestedEntity);
                if (mgrType == null)
                    throw new DeveloperException("Can't find manager for " + requestedEntity);

                var objectType = _managerForObject.GetTypeByName(requestedEntity);
                if (objectType == null)
                    throw new DeveloperException("Can't find type for object " + requestedEntity);

                object result;
                MethodInfo methodInfo;
                ParameterInfo[] methodParams;
                object[] args;

                double lastQueryExecutionTime;

                var mgr = IoC.Instance.Resolve(mgrType, null) as IBaseManager;
                if (mgr == null)
                    throw new DeveloperException("Can't resolve IBaseManager by {0}", mgrType);

                // включаем данный manager в объявленную сессию работы. при этом временем жизни сессии управляем извне
                mgr.SetUnitOfWork(uow);

                // определяем, какой метод нужно выбрать
                methodInfo = GetMethod(mgrType, actionName, parameters);
                if (methodInfo == null)
                    throw new DeveloperException("Can't find method", new MissingMethodException(mgrType.Name, actionName));

                // заполняем коллекцию входных параметров метода
                methodParams = methodInfo.GetParameters();
                args = new object[methodParams.Length];
                for (var i = 0; i < methodParams.Length; i++)
                    args[i] = GetParameterValue(methodParams[i], parameters);

                // вызываем метод
                Log.DebugFormat("Invoke method {0}.{1}", mgrType, methodInfo.Name);
                result = methodInfo.Invoke(mgr, args);
                lastQueryExecutionTime = mgr.LastQueryExecutionTime;

                // обрабатываем результаты работы
                answer.ProcessResults(requestedEntity, result, args, methodInfo, methodParams);

                //Упаковываем секцию statictics
                var statisticValue = lastQueryExecutionTime.ToString(CultureInfo.InvariantCulture);
                answer.AddStatisticNode(statisticValue);
            }
            // ловим все - канал нельзя останавливать
            catch (Exception ex)
            {
                try
                {
                    var newEx = DALExceptionHandler.ProcessException(ex);
                    answer.AddExceptionNode(newEx);
                }
                catch (Exception ex2)
                {
                    Log.Error("Не удалось сформировать сообщение об ошибке: " + ExceptionHelper.GetErrorMessage(ex), ex2);
                }
            }
            finally
            {
                // закрываем короткую сессию
                if (uow != null && disposeUow)
                {
                    Log.DebugFormat("Dispose short UnitOfWork session");
                    uow.Dispose();
                }
                Log.DebugFormat("End processing telegram {0} in {1}", telegram.TransactionNumber, DateTime.Now - startProcessingTime);
            }
            return answer.UnWrap();
        }

        public void Close()
        {
            if (_uowFactory != null)
                _uowFactory.Rollback();
        }
        #endregion

        #region .  Methods  .
        private static MethodInfo GetMethod(Type type, string actionName, List<Node> parameters)
        {
            var paramNames = parameters == null || parameters.Count == 0 ? null : string.Join(string.Empty, parameters.Select(i => i.Name));
            var key = string.Format("{0}.{1}({2})", type.FullName, actionName, paramNames);
            return MethodCache.GetOrAddSafe(key, k => GetMethodInternal(type, actionName, parameters));
        }

        private static MethodInfo GetMethodInternal(Type type, string actionName, List<Node> parameters)
        {
            // получаем все методы
            var methods = type.GetMethods();

            // шаг 1 - ищем по имени
            var methodsWithName = methods.Where(i => actionName.EqIgnoreCase(i.Name)).ToArray();
            if (methodsWithName.Length == 0)
                return null;

            // нашил == 1
            if (methodsWithName.Length == 1)
            {
                // если с параметрами все ок, то отдаем
                if (IsMethodForParams(methodsWithName[0], parameters))
                    return methodsWithName[0];
            }
            else
            {
                return methodsWithName.FirstOrDefault(info => IsMethodForParams(info, parameters));
            }

            return null;
        }

        private static object GetParameterValue(ParameterInfo methodParam, List<Node> telegramParams)
        {
            var paramNode = telegramParams.Find(i => i.Name.Equals(methodParam.Name));
            var realType = methodParam.ParameterType.IsByRef ? methodParam.ParameterType.GetElementType() : methodParam.ParameterType;

            //МЕГА HACK!!!
            if (paramNode.Name == "clientSessionServiceId")
            {
                //paramNode.Value = _id;
            }

            return NodeToValue(paramNode, realType);
        }

        private static object NodeToValue(Node node, Type valueType)
        {
            //Передаем XmlDocument
            if (valueType == typeof(XmlDocument))
            {
                if (node.Value == null)
                    return null;
                var doc = new XmlDocument();
                doc.LoadXml(node.Value);
                return doc;
            }

            // INFO: вынес отдельно от IXmlSerializable чтобы проверить приходит ли документ
            if (typeof(XmlDocument).IsAssignableFrom(valueType))
            {
                if (node.Value == null)
                    return null;
                var doc = new XmlDocument();
                doc.LoadXml(node.Value);
                return XmlDocumentConverter.ConvertTo(valueType, doc);
            }

            // сложный объект пусть сам себя разбирает
            if (typeof(IXmlSerializable).IsAssignableFrom(valueType))
            {
                var doc = new XmlDocument();
                if (node.Value != null)
                    doc.LoadXml(node.Value);
                return node.Value != null ? XmlDocumentConverter.ConvertTo(valueType, doc) : null;
            }

            // если ожидаем коллекцию
            if (IsCollectionType(valueType))
            {
                var itemType = valueType.GetGenericArguments()[0];
                // создаем коллекцию
                var res = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                foreach (var n in node.Nodes)
                {
                    var internalValue = NodeToValue(n, itemType);
                    res.Add(internalValue);
                }
                return res;
            }

            if (valueType.IsNullable() && node.Value == null)
                return null;

            var universalResult = SerializationHelper.ConvertToTrueType(node.Value, valueType);
            if (universalResult != null)
                return universalResult;

            return node.Value;
        }

        private static bool IsMethodForParams(MethodInfo methodInfo, List<Node> parameters)
        {
            var methodParameters = methodInfo.GetParameters();

            // берем все параметры, кроме result-а
            var telegramParameters = parameters.Where(i => !Node.ResultNodeName.EqIgnoreCase(i.Name)).ToArray();

            // быстрый выход
            if (methodParameters.Length != telegramParameters.Length)
                return false;

            foreach (var telParm in telegramParameters)
            {
                //HACK: сейчас используется поиск по имени, т.к. в телеграммах не передаются типы. Правильнее было бы искать по типам и использовать стандартный 
                //в телеграмме есть параметр с таким именем, а в методе?
                if (!methodParameters.Any(i => i.Name.EqIgnoreCase(telParm.Name)))
                    return false;
            }

            return true;
        }

        private static bool IsCollectionType(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type)
                && type != typeof(string)
                && type != typeof(XmlDocument);
        }
        #endregion
    }
}
