using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using wmsMLC.Business.DAL.WebAPI;
using wmsMLC.Business.General;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;
using XmlSerializer = System.Xml.Serialization.XmlSerializer;

namespace wmsMLC.Business.Managers.Imports
{
    public class ImportManager : IImportManager
    {
        private const string RootName = "WMSTELEGRAM";
        private string _apiUri = string.Empty;

        public ImportObject ProcessImport(string xmlString)
        {
            ImportObject impObj = null;
            try
            {
                var result = string.Empty;
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
                {
                    var reader = new XmlTextReader(stream);
                    var serializer = new XmlSerializer(typeof (ImportObject));
                    if (!serializer.CanDeserialize(reader))
                        throw new DeveloperException("Can't deserialize file.");
                    impObj = serializer.Deserialize(reader) as ImportObject;
                    if (impObj == null)
                        throw new DeveloperException("Can't deserialize file.");
                    impObj.Type = TelType.ANSWER;
                    switch (impObj.TelCode)
                    {
                        case TelCodeEnum.WMS_API:
                            result = ProcessApi(impObj);
                            break;
                        case TelCodeEnum.WMS_PROCESS:
                            result = ProcessBP(impObj: impObj, xml: xmlString);
                            break;
                        case TelCodeEnum.WMS_INSERT:
                            result = ProcessCrud(impObj);
                            break;
                    }                    
                    impObj.Content.Items = new XmlNode[1];
                    var doc = new XmlDocument();
                    doc.LoadXml(result);
                    impObj.Content.Items[0] = doc.DocumentElement;                    
                }
            }
            catch(Exception ex)             
            {
                if(impObj == null)
                    throw;
                var doc = new XmlDocument();
                doc.LoadXml("<ERROR></ERROR>");
                if (doc.DocumentElement == null)
                    throw;
                doc.DocumentElement.InnerText = ex.To<string>();
                var tmp = new List<XmlNode>((XmlNode[])impObj.Content.Items.Clone());
                tmp.Add(doc.DocumentElement);
                impObj.Content.Items = tmp.ToArray();
            }
            return impObj;
        }

        public void SetApiUri(string uri)
        {
            _apiUri = uri;
        }

        public void SetCredentials(string userName, string pwd)
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        private string ProcessCrud(ImportObject impObj)
        {
            var mgrForObj = GetManagerForObject();
            var objectList = new Dictionary<string, List<object>>();
            // разбор объектов в контенте
            foreach (var node in impObj.Content.Items)
            {
                // если перед нами элемент
                if (node.NodeType == XmlNodeType.Element)
                {
                    var nodeName = node.Name;
                    // попробуем найти зарегистрированный тип объекта
                    var nodeType = mgrForObj.GetTypeByTENTName(node.Name);
                    object obj;
                    // если перед нами известный тип объекта, то получим его инстанс
                    if (nodeType != null)
                    {
                        //nodeName = nodeType.Name;
                        obj = NodeToValue(node, nodeType);                        
                    }
                    else // иначе будем перебирать все входящие в него элементы
                    {
                        obj = node.Value;
                    }
                    AddToDictionary(objectList, nodeName, obj);
                }
            }

            var objType = GetEntityTypeByTent(mgrForObj, impObj);
            var mgrType = mgrForObj.GetManagerByTypeName(objType.Name);
            if (mgrType == null)
                throw new DeveloperException("Can't find manager for " + impObj.Entity);
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();
            using (var uow = uowFactory.Create(false))
            {
                var mgr = IoC.Instance.Resolve(mgrType, null) as IBaseManager;
                if (mgr != null)
                {
                    mgr.SetUnitOfWork(uow);
                    try
                    {
                        uow.BeginChanges();
                        // TODO: реализовать нормально поддержку IEnumerable
                        //mgr.Insert(ref objectList);
                        foreach (var p in objectList[impObj.Entity])
                        {
                            var o = p;
                            mgr.Insert(ref o);
                        }
                        uow.CommitChanges();
                    }
                    catch(Exception ex)
                    {
                        uow.RollbackChanges();
                        throw new DeveloperException("Произошла ошибка во время обработки телеграммы: {0}", ex.Message);
                    }
                    finally
                    {
                        mgr.Dispose();
                    }
                }
            }
            return null;
        }

        private string ProcessApi(ImportObject impObj)
        {
            if (string.IsNullOrEmpty(_apiUri))
                throw new OperationException("Не указана точка соединения для сервиса API.");

            var objType = GetEntityTypeByTent(GetManagerForObject(), impObj);

            if (impObj.Content != null)
            {
                var items = impObj.Content.Items.FirstOrDefault(i => i.Name.EqIgnoreCase("ITEMS"));
                if (items != null && !string.IsNullOrEmpty(items.InnerText.Trim()))
                {
                    var action = impObj.Action;
                    if (string.IsNullOrEmpty(action))
                        throw new DeveloperException("Action is not defined.");

                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(items.OuterXml);
                    var helper = new IntegrationServiceImpl(_apiUri);
                    return helper.ProcessApi(action, objType.Name, xmlDoc) ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        private string ProcessBP(ImportObject impObj, string xml)
        {
            if (string.IsNullOrEmpty(impObj.Action)) 
                throw new DeveloperException("Action not set.");
            var bpMgr = IoC.Instance.Resolve<IBPProcessManager>();
            if (bpMgr == null)
                throw new DeveloperException("IBPProcessManager not registered.");
            
            var context = new BpContext { Items = impObj.Content.Items };
            context.Set("DialogVisible", true); // признак того, что надо показывать диалоги
            context.Set(RootName, xml);
            bpMgr.Parameters.Add(BpContext.BpContextArgumentName, context);
            CompleteContext resCtx = null;
            bpMgr.Run(code: impObj.Action, completedHandler: ctx => { WaitHandle.Set(); resCtx = ctx; });
            WaitHandle.WaitOne();
            var result = string.Empty;
            if (resCtx != null)
            {
                if (resCtx.Exception != null)
                {
                    //Избавляемся от & в сообщении об ошибке
                    var errmessage = resCtx.Exception.Message;
                    if (!string.IsNullOrEmpty(errmessage))
                        errmessage = errmessage.Replace("&", "&amp;");
                    result = string.Format("<ERROR>{0}</ERROR>", errmessage);
                }
                else
                {
                    // оставил старую логику
                    result = resCtx.Parameters.ContainsKey("RESULT") ? resCtx.Parameters["RESULT"].ToString() : null;
                }
            }
            if (result != null)
                return result;
            return "<STATUS>OK</STATUS>";
        }

        private IManagerForObject GetManagerForObject()
        {
            var mgrForObj = IoC.Instance.Resolve<IManagerForObject>();
            if (mgrForObj == null)
                throw new DeveloperException("Can't find manager for object.");
            return mgrForObj;
        }

        private Type GetEntityTypeByTent(IManagerForObject mgrForObj, ImportObject impObj)
        {
            if (mgrForObj == null)
                throw new ArgumentNullException("mgrForObj");
            if (impObj == null)
                throw new ArgumentNullException("impObj");

            var objType = mgrForObj.GetTypeByTENTName(impObj.Entity);
            if (objType == null)
                throw new DeveloperException("Can't find type '{0}'.", impObj.Entity);
            return objType;
        }

        private void AddToDictionary(Dictionary<string, List<object>> dict, string key, object obj)
        {
            // если объект не пустой, то добавим
            if (obj != null)
            {
                if (!dict.ContainsKey(key))
                {
                    var list = new List<object> {obj};
                    dict.Add(key, list);
                }
                else
                {
                    dict[key].Add(obj);
                }
            }
        }

        private object NodeToValue(XmlNode node, Type valueType)
        {
            // сложный объект пусть сам себя разбирает
            if (typeof(IXmlSerializable).IsAssignableFrom(valueType))
            {
                var doc = new XmlDocument();
                doc.LoadXml(node.OuterXml);
                return XmlDocumentConverter.ConvertTo(valueType, doc);
            }

            // если ожидаем коллекцию
            if (typeof(IEnumerable).IsAssignableFrom(valueType) && valueType != typeof(string))
            {
                var itemType = valueType.GetGenericArguments()[0];
                // создаем коллекцию
                var res = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
                foreach (XmlNode n in node.ChildNodes)
                {
                    var internalValue = NodeToValue(n, itemType);
                    res.Add(internalValue);
                }
                return res;
            }

            return node.Value;
        }

        public CompleteContext ExecuteWfByCode(string wfcode, Dictionary<string, object> parameters, TimeSpan? timeout)
        {
            var executionContext = new Objects.Processes.ExecutionContext(string.Empty, parameters);

            var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
            CompleteContext resCtx = null;
            engine.Run(workflowXaml: wfcode, context: executionContext, completedHandler: ctx =>
            {
                WaitHandle.Set(); 
                resCtx = ctx;
            });

            if (timeout.HasValue)
                WaitHandle.WaitOne(timeout.Value);
            else
                WaitHandle.WaitOne();

            return resCtx;
        }
    }
}
