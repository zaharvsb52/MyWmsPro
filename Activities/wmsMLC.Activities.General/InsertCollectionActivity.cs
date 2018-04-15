using System.Activities;
using System.Collections.Generic;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class InsertCollectionActivity<T> : BaseCRUDActivity<T>
    {
        /// <summary>
        /// Объекты, который хотим изменить
        /// </summary>
        [Required]
        public InArgument<IEnumerable<object>> Keys { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var manager = GetBaseManager();
            var res = (IEnumerable<T>) Keys.Get(context);
            manager.Insert(ref res);
        }
    }
}
