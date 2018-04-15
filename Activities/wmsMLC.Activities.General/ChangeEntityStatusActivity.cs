using System;
using System.Activities;
using System.ComponentModel;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class ChangeEntityStatusActivity<T> : BaseCRUDActivity<T, T>
    {
        /// <summary>
        /// Ключ объекта, статус которого изменяем.
        /// </summary>
        [Required]
        [DisplayName(@"Ключ объекта, статус которого изменяем")]
        public InArgument<object> Key { get; set; }

        /// <summary>
        /// Операция.
        /// </summary>
        [Required]
        [DisplayName(@"Операция")]
        public InArgument<string> Operation { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var manager = GetBaseManager();
            var stateMgr = manager as IStateChange;
            if (stateMgr == null)
                throw new NotImplementedException("IStateChange");

            var uow = BeginTransactionActivity.GetUnitOfWork(context);
            if (uow != null)
                manager.SetUnitOfWork(uow);

            var key = Key.Get(context);
            stateMgr.ChangeStateByKey(key, Operation.Get(context));
            context.SetValue(Result, manager.Get(key));
        }
    }
}
