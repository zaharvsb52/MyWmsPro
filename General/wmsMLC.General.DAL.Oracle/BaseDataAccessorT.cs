using System;
using BLToolkit.Data;
using BLToolkit.DataAccess;

namespace wmsMLC.General.DAL.Oracle
{
    /// <summary>
    /// Abstraction on DataAccessorT.
    /// </summary>
    public abstract class BaseDataAccessor<T> : DataAccessor<T>, IUnitOfWorkUser, IDisposable
    {
        #region .  Fields  .
        private UnitOfWork _unitOfWork;
        private bool _disposeUnitOfWork;
        #endregion

        #region .  Methods  .
        public void SetUnitOfWork(IUnitOfWork uow, bool dispose)
        {
            //Contract.Requires<ArgumentNullException>(uow != null);

            _unitOfWork = (UnitOfWork)uow;
            _disposeUnitOfWork = dispose;
        }

        protected IUnitOfWork GetUnitOfWork()
        {
            return _unitOfWork;
        }

        protected override DbManager CreateDbManager()
        {
            if (_unitOfWork != null && _unitOfWork.HasDbManager())
            {
                DisposeDbManager = _disposeUnitOfWork;
                return _unitOfWork.DbManager;
            }

            var db = new BaseDBManager();
            if (_unitOfWork != null)
                db.SetParametersFromUoW(_unitOfWork);
            return db;
        }
        #endregion

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            finally
            {
                _disposed = true;
            }
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">False - если требуется освободить только UnManaged ресурсы, True - если еще и Managed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_disposeUnitOfWork && _unitOfWork != null)
                {
                    _unitOfWork.Dispose();
                    _unitOfWork = null;
                }

                Dispose(DbManager);
            }
        }
        #endregion
    }
}