using System;
using System.Data;
using wmsMLC.General.Resources;

namespace wmsMLC.General.DAL
{
    public abstract class BaseUnitOfWork : IUnitOfWork
    {
        #region .  Variables  .

        private int _transactUseCount;

        private readonly Guid _id;

        private bool _disposed;

        #endregion

        #region .  Properties  .
        public virtual int? TimeOut { get; set; }
        public int? SessionId { get; set; }
        public string UserSignature { get; set; }
        private int TransactUseCount
        {
            get { return _transactUseCount; }
            set
            {
                _transactUseCount = value;
                if (_transactUseCount < 0)
                    throw new Exception(ExceptionResources.TransactionError);
            }
        }
        #endregion

        #region .  ctr  .

        protected BaseUnitOfWork()
        {
            //AddTrace();
        }

        protected BaseUnitOfWork(UnitOfWorkContext context) : this()
        {
            _id = context.Id;
            SessionId = context.SessionId;
            UserSignature = context.UserSignature;
            TimeOut = context.TimeOut;
        }

        #endregion

        #region .  IUnitOfWork Members  .

        public bool IsInTransaction()
        {
            return TransactUseCount > 0;
        }

        public void BeginChanges()
        {
            TransactUseCount++;
            if (TransactUseCount > 1)
                return;

            BeginChanges_Internal();

            if (Began != null)
                Began(this, new EventArgs());
        }

        public void BeginChanges(IsolationLevel isolationLevel)
        {
            //if (_id == Guid.Empty)
            //    throw new DeveloperException("Использование транзакций в коротких сессиях");

            TransactUseCount++;
            if (TransactUseCount > 1)
                return;

            BeginChanges_Internal(isolationLevel);

            if (Began != null)
                Began(this, new EventArgs());
        }

        public void CommitChanges()
        {
            TransactUseCount--;
            if (TransactUseCount > 0)
                return;

            CommitTransaction();

            if (Committed != null)
                Committed(this, new EventArgs());
        }

        public void RollbackChanges()
        {
            RollbackTransaction();

            TransactUseCount = 0;

            if (Rolled != null)
                Rolled(this, new EventArgs());
        }

        public Guid GetId()
        {
            return _id;
        }

        protected abstract void BeginChanges_Internal();
        protected abstract void BeginChanges_Internal(IsolationLevel isolationLevel);
        protected abstract void CommitTransaction();
        protected abstract void RollbackTransaction();

        public event EventHandler<EventArgs> Began;
        public event EventHandler<EventArgs> Committed;
        public event EventHandler<EventArgs> Rolled;
        public event EventHandler<EventArgs> Disposed;

        #endregion

        #region .  IDisposable Members  .

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">Признак, показывающий откуда был вызов - false - finalizer. Пока оставляю для возможности освобождать unmanaged ресурсы</param>
        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            try
            {
                if (_disposed)
                    return;

                Dispose(true);
                OnDispose();
            }
            finally
            {
                _disposed = true;
            }
        }

        private void OnDispose()
        {
            var h = Disposed;
            if (h != null)
                h(this, new EventArgs());
        }

        #endregion

/*
        #region .  Debug instances traser  .
        private static volatile int _instanceCount = 0;
        private static readonly ConcurrentDictionary<IUnitOfWork, string> _activeInstancesStack = new ConcurrentDictionary<IUnitOfWork, string>();
        private readonly ILog _log = LogManager.GetLogger(typeof(BaseUnitOfWork));
        private static bool IsInstanceTraceEnable = true;

        private void AddTrace()
        {
            if (!IsInstanceTraceEnable)
                return;

            _instanceCount++;
            _activeInstancesStack.TryAdd(this, GetCallStack());
        }

        private void RemoveTrace()
        {
            if (!IsInstanceTraceEnable)
                return;

            _instanceCount--;
            string res;
            _activeInstancesStack.TryRemove(this, out res);
            _log.DebugFormat("Dispose uow. Active instances = {0} ({1})", _instanceCount, _activeInstancesStack.Count);
            if (_instanceCount == 0 && _activeInstancesStack.Count > 0)
            {
                //foreach (var i in _activeInstancesStack.Values)
                //{
                //    _log.Debug("uow stack:\n\r" + _activeInstancesStack);
                //}
            }
        }

        private static string GetCallStack()
        {
            var frames = new StackTrace().GetFrames();
            return frames == null ? null : frames.Aggregate(string.Empty, (current, f) => current + f.GetMethod() + Environment.NewLine);
        } 
        #endregion
*/
    }
}