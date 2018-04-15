using System.Activities;
using System.Collections.Generic;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class DeleteCollectionActivity<T> : BaseCRUDActivity<T>
    {
        /// <summary>
        /// Объекты, которые хотим удалить
        /// </summary>
        [Required]
        public InArgument<IEnumerable<T>> Keys { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var manager = GetBaseManager();
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                manager.SetUnitOfWork(uow);
            manager.Delete(Keys.Get(context));
        }
    }
}