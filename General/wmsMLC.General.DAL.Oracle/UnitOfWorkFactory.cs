using System;
using System.Collections.Concurrent;
using System.Linq;
using log4net;

namespace wmsMLC.General.DAL.Oracle
{
    public sealed class UnitOfWorkFactory : BaseUnitOfWorkFactory<UnitOfWork>
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (UnitOfWorkFactory));
        private readonly ConcurrentDictionary<Guid, UnitOfWork> _collection = new ConcurrentDictionary<Guid, UnitOfWork>();

        protected override UnitOfWork Create_Internal(UnitOfWorkContext context)
        {
            LogUnitOfWorkQueryCount();
            // если короткая сессия - не запоминаем
            if (context == null || context.Id.Equals(Guid.Empty))
                return new UnitOfWork(context);

            if (_collection.ContainsKey(context.Id))
                return _collection[context.Id];

            var uow = new UnitOfWork(context);
            uow.Disposed += uow_Disposed;
            _collection.TryAdd(context.Id, uow);
            return uow;
        }

        private void uow_Disposed(object sender, EventArgs e)
        {
            var uow = (UnitOfWork)sender;
            uow.Disposed -= uow_Disposed;
            _collection.TryRemove(uow.GetId(), out uow);
            LogUnitOfWorkQueryCount();
        }

        private void LogUnitOfWorkQueryCount()
        {
//#if DEBUG
//            _log.DebugFormat("Uow count = {0}", _collection.Count);
//#endif
        }

        public override void Rollback()
        {
            foreach (var unitOfWork in _collection.Where(p => p.Value != null))
            {
#if DEBUG
                    _log.DebugFormat("Rollback Uow {0}", unitOfWork.Value.GetId());
#endif
                    unitOfWork.Value.RollbackChanges();
                    unitOfWork.Value.Dispose();
            }
            base.Rollback();
        }
    }
}