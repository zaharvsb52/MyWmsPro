using System.Activities;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.General
{
    public class GetByFilterActivity<T> : BaseCRUDActivity<T, List<T>>
    {
        /// <summary>
        /// Строка фильтра
        /// </summary>
        public InArgument<string> Filter { get; set; }

        /// <summary>
        /// Режим получения
        /// </summary>
        public GetModeEnum Mode { get; set; }

        /// <summary>
        /// Атрибуты сущности
        /// </summary>
        public string AttrEntity { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var itemType = typeof (T);
            var isHistory = itemType.IsGenericType && typeof (IHistoryItem).IsAssignableFrom(itemType);

            if (isHistory)
                itemType = itemType.GetGenericArguments()[0];

            var mgrInterface = typeof (IBaseManager<>).MakeGenericType(itemType);
            var mgr = (IBaseManager)IoC.Instance.Resolve(mgrInterface);
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                mgr.SetUnitOfWork(uow);
            mgr.ClearCache();

            var filter = Filter.Get(context);
            List<object> result;
            if (isHistory)
            {
                var historyMgr = (IHistoryManager) mgr;
                var res = string.IsNullOrEmpty(AttrEntity)
                    ? historyMgr.GetHistory(filter, Mode).ToList()
                    : historyMgr.GetHistory(filter, AttrEntity).ToList();
                result = res;
            }
            else
            {
                result = string.IsNullOrEmpty(AttrEntity)
                    ? mgr.GetFiltered(filter, Mode).ToList()
                    : mgr.GetFiltered(filter, AttrEntity).ToList();
            }

            context.SetValue(Result, result.Cast<T>().ToList());
        }
    }
}