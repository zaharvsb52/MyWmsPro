using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Activities.General
{
    public class UpdateActivity<T> : BaseCRUDActivity<T>
    {
        /// <summary>
        /// Объект, который хотим изменить
        /// </summary>
        [Required]
        //АРГУМЕНТ ДОЛЖЕН БЫТЬ INOUT
        public InOutArgument<T> Key { get; set; }

        [DisplayName(@"Не сохранять вложенные сущности")]
        [DefaultValue(false)]
        public bool IsNotUpdateInnerEntities { get; set; }

        [DisplayName(@"Как новый объект")]
        public bool IsNew { get; set; }

        public UpdateActivity()
        {
            IsNew = false;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Key, type.ExtractPropertyName(() => Key));
            
            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var manager = GetBaseManager();
            var uw = BeginTransactionActivity.GetUnitOfWork(context);
            if (uw != null)
                manager.SetUnitOfWork(uw);

            var item = Key.Get(context);

            if (!IsNotUpdateInnerEntities)
            {
                var serializable = item as ICustomXmlSerializable;
                if (serializable != null)
                    serializable.OverrideIgnore = false;
            }

            var isNew = item as IIsNew;
            if (IsNew || isNew != null && isNew.IsNew)
            {
                manager.Insert(ref item);
                context.SetValue(Key, item);
            }
            else
            {
                manager.Update(Key.Get(context));
            }
        }
    }
}