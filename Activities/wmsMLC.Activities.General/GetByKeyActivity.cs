using System.Activities;
using wmsMLC.General;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class GetByKeyActivity<T> : BaseCRUDActivity<T, T>
    {
        /// <summary>
        /// Ключ.
        /// </summary>
        [Required]
        public InArgument<object> Key { get; set; }

        /// <summary>
        /// Режим получения
        /// </summary>
        public GetModeEnum Mode { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var key = Key.Get(context);
            if (key == null)
                throw new DeveloperException("Не задан обазательный параметр Key");

            var manager = GetBaseManager();
            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                manager.SetUnitOfWork(uow);

            var result = manager.Get(key, Mode);

            context.SetValue(Result, result);
        }
    }
}