using System.Activities;
using wmsMLC.Business.General;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.General
{
    public abstract class BaseCRUDActivity<T> : NativeActivity
    {
        protected BaseCRUDActivity()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected IBaseManager<T> GetBaseManager()
        {
            var service = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = service.GetManagerByTypeName(typeof(T).Name);

            var manager = IoC.Instance.Resolve(mgrType, null) as IBaseManager<T>;
            if (manager == null)
                throw new DeveloperException("Can't resolve IBaseManager<T> by " + mgrType.Name);

            return manager;
        }
    }

    public abstract class BaseCRUDActivity<T, TResult> : NativeActivity<TResult>
    {
        protected BaseCRUDActivity()
        {
            this.DisplayName = GetType().Name.Replace("Activity", string.Empty);
        }

        protected IBaseManager<T> GetBaseManager()
        {
            var service = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = service.GetManagerByTypeName(typeof(T).Name);

            var manager = IoC.Instance.Resolve(mgrType, null) as IBaseManager<T>;
            if (manager == null)
                throw new DeveloperException("Can't resolve IBaseManager<T> by " + mgrType.Name);

            return manager;
        }
    }
}