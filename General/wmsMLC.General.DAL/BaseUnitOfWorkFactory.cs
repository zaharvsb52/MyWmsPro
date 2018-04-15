using System;

namespace wmsMLC.General.DAL
{
    public abstract class BaseUnitOfWorkFactory<T> : IUnitOfWorkFactory
        where T : IUnitOfWork
    {
        public IUnitOfWork Create(bool isShortSession, int? timeOut)
        {
            var defaultContext = new UnitOfWorkContext
            {
                TimeOut = timeOut,
                UserSignature = WMSEnvironment.Instance.AuthenticatedUser == null
                    ? null
                    : WMSEnvironment.Instance.AuthenticatedUser.GetSignature(),
                SessionId = !WMSEnvironment.Instance.SessionId.HasValue
                    ? null
                    : (int?) WMSEnvironment.Instance.SessionId.Value
            };
            if (!isShortSession)
                defaultContext.Id = Guid.NewGuid();
            return Create(defaultContext);
        }

        public IUnitOfWork Create(UnitOfWorkContext context)
        {
            return Create_Internal(context);
        }

        public virtual void Rollback()
        {
        }

        protected virtual T Create_Internal(UnitOfWorkContext context)
        {
            if (context == null)
                return Activator.CreateInstance<T>();

            return (T)Activator.CreateInstance(typeof(T), context);
        }
    }
}