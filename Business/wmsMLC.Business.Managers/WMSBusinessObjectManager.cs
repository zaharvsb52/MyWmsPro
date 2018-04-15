using System.Collections.Generic;
using wmsMLC.Business.DAL;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Managers
{
    public class WMSBusinessObjectManager<T, TKey> : SecurityManager<T, TKey>, IHistoryManager<T>, IHistoryManager, IStateChange where T : WMSBusinessObject
    {
        public const string GetHistoryRightName = "H";

        public IEnumerable<HistoryWrapper<T>> GetHistory(string filter, GetModeEnum mode)
        {
            return GetHistory(filter, GetAttrEntity(typeof (T), mode));
        }

        public IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity = null)
        {
            Check(GetHistoryRightName);
            using(var repo = GetRepository<IHistoryRepository<T>>())
                return repo.GetHistory(filter, attrentity);
        }

        public void ChangeState(object entity, string operationName)
        {
            var kh = entity as IKeyHandler;
            if (kh == null)
                throw new DeveloperException("Try to change state for non KeyHandler entity");

            ChangeStateByKey((TKey)kh.GetKey(), operationName);
        }

        public void ChangeStateByKey(object entityKey, string operationName)
        {
            using (var repo = GetRepository())
                repo.ChangeStateByKey(entityKey, operationName);
        }

        public IBaseManager<TModel> GetManager<TModel>() where TModel : BusinessObject
        {
            var res = IoC.Instance.Resolve<IBaseManager<TModel>>();
            if (CurrentUnitOfWork != null)
                res.SetUnitOfWork(CurrentUnitOfWork);
            return res;
        }

        IEnumerable<object> IHistoryManager.GetHistory(string filter, string attrentity)
        {
            return GetHistory(filter, attrentity);
        }
        IEnumerable<object> IHistoryManager.GetHistory(string filter, GetModeEnum mode)
        {
            return GetHistory(filter, mode);
        }
    }

    public interface IHistoryManager<T> : IBaseManager<T>
    {
        IEnumerable<HistoryWrapper<T>> GetHistory(string filter, GetModeEnum mode);
        IEnumerable<HistoryWrapper<T>> GetHistory(string filter, string attrentity = null);
    }
}