using System;
using System.Data;

namespace wmsMLC.General.DAL.Oracle
{
    public class UnitOfWork : BaseUnitOfWork
    {
        #region .  Fields  .
        private Lazy<BaseDBManager> _dbManager;
        private bool _inDispose;
        private bool _disposeFormDbManagerEvent;
        #endregion

        #region .  ctr  .

        internal UnitOfWork()
        {
            _dbManager = new Lazy<BaseDBManager>();
        }

        internal UnitOfWork(UnitOfWorkContext context) : base(context)
        {
            ConfigurationString = context.ConfigurationString;

            _dbManager = new Lazy<BaseDBManager>(CreateDbManager);
        }

        #endregion

        internal BaseDBManager DbManager
        {
            get { return _dbManager.Value; }
        }

        public string ConfigurationString { get; set; }

        public override int? TimeOut
        {
            get
            {
                return base.TimeOut;
            }
            set
            {
                base.TimeOut = value;

                // пробрасываем в dbManager
                if (_dbManager != null && _dbManager.IsValueCreated)
                    _dbManager.Value.TimeOut = value;
            }
        }


        #region .  IUnitOfWork Members  .

        protected override void BeginChanges_Internal()
        {
            DbManager.BeginTransaction();
        }

        protected override void BeginChanges_Internal(IsolationLevel isolationLevel)
        {
            DbManager.BeginTransaction(isolationLevel);
        }

        protected override void CommitTransaction()
        {
            DbManager.CommitTransaction();
        }

        protected override void RollbackTransaction()
        {
            DbManager.RollbackTransaction();
        }

        #endregion

        internal bool HasDbManager()
        {
            return _dbManager.IsValueCreated;
        }

        private BaseDBManager CreateDbManager()
        {
            var db = !string.IsNullOrEmpty(ConfigurationString)
                        ? new BaseDBManager(ConfigurationString)
                        : new BaseDBManager();
            db.SetParametersFromUoW(this);
            db.Disposed += DbManager_Disposed;
            return db;
        }

        private void DbManager_Disposed(object sender, EventArgs e)
        {
            var dbManager = (BaseDBManager)sender;
            dbManager.Disposed -= DbManager_Disposed;
            _disposeFormDbManagerEvent = true;

            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_inDispose)
                    return;

                try
                {
                    _inDispose = true;

                    if (_dbManager.IsValueCreated)
                    {
                        if (!_disposeFormDbManagerEvent)
                            _dbManager.Value.Dispose();
                        _dbManager = null;
                    }
                }
                finally
                {
                    _inDispose = false;
                }
            }

            base.Dispose(disposing);
        }
    }
}