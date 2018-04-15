using System;
using System.Data;
using System.Threading.Tasks;
using wmsMLC.General.DAL.Service.Telegrams;
using wmsMLC.General.Resources;
using wmsMLC.General.Services;

namespace wmsMLC.General.DAL.Service
{
    public class UnitOfWork : BaseUnitOfWork
    {
        internal UnitOfWork() { }
        internal UnitOfWork(UnitOfWorkContext context) : base(context) { }

        protected override void Dispose(bool disposing)
        {
            // сервису сообщаем о закрытии только долгих
            if (disposing && !Guid.Empty.Equals(GetId()))
            {
                var telegram = new RepoQueryTelegramWrapper(typeof (IUnitOfWork).Name, "Dispose", new TransmitterParam[0]);
                ProcessTelegramm(telegram);
            }

            base.Dispose(disposing);
        }

        protected override void BeginChanges_Internal()
        {
            var telegram = new RepoQueryTelegramWrapper(typeof(IUnitOfWork).Name, "BeginChanges", new TransmitterParam[0]);
            ProcessTelegramm(telegram);
        }

        protected override void BeginChanges_Internal(IsolationLevel isolationLevel)
        {
            var uowParam = new TransmitterParam { Name = "IsolationLevel", Type = typeof(IsolationLevel), Value = isolationLevel };
            var telegram = new RepoQueryTelegramWrapper(typeof(IUnitOfWork).Name, "BeginChanges", new[] { uowParam });
            ProcessTelegramm(telegram);
        }

        protected override void CommitTransaction()
        {
            var telegram = new RepoQueryTelegramWrapper(typeof(IUnitOfWork).Name, "CommitChanges", new TransmitterParam[0]);
            ProcessTelegramm(telegram);
        }

        protected override void RollbackTransaction()
        {
            var telegram = new RepoQueryTelegramWrapper(typeof(IUnitOfWork).Name, "RollbackChanges", new TransmitterParam[0]);
            ProcessTelegramm(telegram);
        }

#if DEBUG
        private static int DefaultTimeout = 300000;
#else
        private static int DefaultTimeout = 30000;
#endif

        private void ProcessTelegramm(Telegram telegram, TelegramBodyType bodyType = TelegramBodyType.Wms)
        {
            // выставляем uow для управления транзакциями
            telegram.UnitOfWork = GetId();
            // Непосредственную отправку делаем в отдельном потоке.
            // Такой подход необходим, т.к. если отправка будет осуществляться из UI, то произойдет DeadLock
            // Бонусом получаем удобный механизм отслеживания TimeOut-ов
            var processTask = new Task(() =>
                {
                    using (var transmitter = IoC.Instance.Resolve<ITransmitter>())
                    {
                        transmitter.Process(bodyType, telegram);
                    }
                });

            processTask.Start();
            if (!processTask.Wait(DefaultTimeout))
                throw new TimeoutException(ExceptionResources.TimeoutExceptionMessage);
        }
    }
}