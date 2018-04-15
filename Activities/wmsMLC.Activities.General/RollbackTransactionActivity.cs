using System;
using System.Activities;
using log4net;

namespace wmsMLC.Activities.General
{
    public class RollbackTransactionActivity : NativeActivity
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(WriteLog4NetActivity));

        protected override void Execute(NativeActivityContext context)
        {
            // откат транзакции не должен падать с ошибкой
            try
            {
                BeginTransactionActivity.RollbackChanges(context);
            }
            //TODO: правильная ошибка
            catch (Exception ex)
            {
                _log.ErrorFormat("Ошибка отмены транзакции. {0}", ex.Message);
                _log.Debug(ex);
            }
        }
    }
}
