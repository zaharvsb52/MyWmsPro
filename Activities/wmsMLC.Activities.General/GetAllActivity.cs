using System.Activities;
using System.Collections.Generic;
using System.Linq;
using wmsMLC.General;

namespace wmsMLC.Activities.General
{
    public class GetAllActivity<T> : BaseCRUDActivity<T, List<T>>
    {
        /// <summary>
        /// Режим получения
        /// </summary>
        public GetModeEnum Mode { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            //TODO: сделать возможность часичного получения
            var manager = GetBaseManager();
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                manager.SetUnitOfWork(uow);
            context.SetValue(Result, manager.GetAll(Mode).ToList());
        }
    }
}