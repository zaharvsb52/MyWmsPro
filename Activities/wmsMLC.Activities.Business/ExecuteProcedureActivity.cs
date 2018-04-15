using System;
using System.Activities;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using log4net;
using wmsMLC.Activities.General;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.Activities.Business
{
    /// <summary>
    /// Активити запуска метода вызова API DB
    /// </summary>
    public sealed class ExecuteProcedureActivity : ExecuteMethodActivity<IBPProcessManager>
    {
        public override MethodInfo[] GetMethods()
        {
            var all = base.GetMethods();
            return all.Where(i => i.GetOneCustomAttributes<BPAttribute>() != null).ToArray();
        }
    }

    /// <summary>
    /// Абстрактный класс, реализующий логику выбора, передачи параметров и запуска метода
    /// </summary>
    /// <typeparam name="T">Объект, методы которого необходимо запускать</typeparam>
    public abstract class ExecuteMethodActivity<T> : NativeActivity<bool>, IExecuteMethodActivity
        where T : class, ITrueBaseManager
    {
        #region .  Consts & fields .
        public const string ValuePropertyName = "Value";
        public const string ResultParamName = "result";
        public const string ExceptionPropertyName = "Exception";

        private readonly ILog _log = LogManager.GetLogger(typeof(ExecuteMethodActivity<T>));
        #endregion

        #region .  Arguments & Properties  .

        [DisplayName(@"Имя метода")]
        public InArgument<string> Value { get; set; }

        [DisplayName(@"Время ожидания выполнения (мсек)")]
        [DefaultValue(null)]
        public InArgument<int?> TimeOut { get; set; }

        [DisplayName(@"Ошибка")]
        public OutArgument<Exception> Exception { get; set; }

        [DisplayName(@"Параметры метода")]
        public Dictionary<string, Argument> Parameters { get; private set; }

        #endregion

        protected ExecuteMethodActivity()
        {
            DisplayName = "Запуск метода";
            Parameters = new Dictionary<string, Argument>();
        }

        #region .  Methods  .

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            foreach (var key in Parameters.Keys)
            {
                var argument = Parameters[key];
                var runtimeArgument = new RuntimeArgument(key, argument.ArgumentType, argument.Direction);
                metadata.Bind(argument, runtimeArgument);
                collection.Add(runtimeArgument);
            }

            var aProperty = new RuntimeArgument(ValuePropertyName, typeof(string), ArgumentDirection.In, true);
            metadata.Bind(Value, aProperty);
            collection.Add(aProperty);

            var aTimeOut = new RuntimeArgument("TimeOut", typeof(int?), ArgumentDirection.In, true);
            metadata.Bind(TimeOut, aTimeOut);
            collection.Add(aTimeOut);

            var exceptionProperty = new RuntimeArgument(ExceptionPropertyName, typeof(Exception), ArgumentDirection.Out, false);
            metadata.Bind(Exception, exceptionProperty);
            collection.Add(exceptionProperty);

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var methodName = Value.Get(context);

            if (ActivityHelpers.UseActivityStackTrace(context))
            {
                var tracking = new CustomTrackingRecord(DisplayName);
                tracking.Data.Add("Value", string.Format("Method='{0}'", methodName));
                context.Track(tracking);
            }

            IUnitOfWork uow = null;
            var isNeedDisposeUoW = false;
            try
            {
                // определяем был ли объявлен UoW
                uow = BeginTransactionActivity.GetUnitOfWork(context);

                int? timeOut = TimeOut.Get(context);
                // если не создали - делаем сами
                if (uow == null)
                {
                    uow = UnitOfWorkHelper.GetUnit(true);
                    isNeedDisposeUoW = true;
                }

                if (timeOut.HasValue)
                    uow.TimeOut = timeOut;

                var mgr = IoC.Instance.Resolve<T>();
                if (uow != null)
                    mgr.SetUnitOfWork(uow);

                // вызываем нужный метод
                var isResultParamExists = Parameters.ContainsKey(ResultParamName);

                var argsCount = Parameters.Count - (isResultParamExists ? 1 : 0);
                var args = new object[argsCount];
                var methodInfo = mgr.GetType().GetMethod(methodName);
                var methodParams = methodInfo.GetParameters();
                for (var i = 0; i < argsCount; i++)
                    args[i] = Parameters[methodParams[i].Name].Get(context);

                var isNeedResult = methodInfo.ReturnType != typeof(void);
                if (!isNeedResult && isResultParamExists)
                    throw new DeveloperException(
                        "Метод {0} не возвращает результат, однако найден параметр с именем {1}", methodName,
                        ResultParamName);

                // запускаем метод
                var resultValue = methodInfo.Invoke(mgr, args);

                // получаем результаты
                if (isNeedResult)
                    Parameters[ResultParamName].Set(context, resultValue);

                for (int i = 0; i < args.Length; i++)
                {
                    var element = Parameters.ElementAt(i);
                    var argument = Parameters.ElementAt(i).Value;
                    if ((argument.Direction == ArgumentDirection.Out ||
                         argument.Direction == ArgumentDirection.InOut) & !element.Key.EqIgnoreCase(ResultParamName))
                        argument.Set(context, args[i]);
                }

                Result.Set(context, true);
            }
            catch (Exception ex)
            {
                Result.Set(context, false);
                Exception.Set(context, ex);
                _log.Warn("Ошибка запуска метода. Причина: " + ExceptionHelper.GetErrorMessage(ex), ex);
            }
            finally
            {
                if (isNeedDisposeUoW && uow != null)
                {
                    try
                    {
                        uow.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _log.Warn("Не удалось закрыть сессию UoW. Причина: " + ExceptionHelper.GetErrorMessage(ex), ex);
                    }
                }
            }
        }

        public virtual MethodInfo[] GetMethods()
        {
            var obj = IoC.Instance.Resolve<T>();
            return obj.GetType().GetMethods();
        }

        #endregion
    }

    public interface IExecuteMethodActivity
    {
        MethodInfo[] GetMethods();
    }
}
