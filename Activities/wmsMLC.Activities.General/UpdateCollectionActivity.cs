using System.Activities;
using System.Collections.Generic;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class UpdateCollectionActivity<T> : BaseCRUDActivity<T>
    {
        /// <summary>
        /// Объекты, который хотим изменить
        /// </summary>
        [Required]
        public InArgument<IEnumerable<object>> Keys { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var manager = GetBaseManager();

            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                manager.SetUnitOfWork(uw);

            manager.Update(Keys.Get(context));
        }
    }
}