using System.Activities;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class DeleteActivity<T> : BaseCRUDActivity<T>
    {
        /// <summary>
        /// Объект, который хотим удалить
        /// </summary>
        [Required]
        public InArgument<T> Key { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var manager = GetBaseManager();
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                manager.SetUnitOfWork(uow);
            manager.Delete(Key.Get(context));
        }
    }
}