using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;

namespace wmsMLC.Activities.General
{
    public class SerializeActivity<T> : NativeActivity
    {
        [DisplayName(@"Объект")]
        [DefaultValue(null)]
        public InArgument<T> Object { get; set; }
        
        [DisplayName(@"Коллекция объектов")]
        [DefaultValue(null)]
        public InArgument<IEnumerable<T>> Collection { get; set; }

        public OutArgument<string> Result { get; set; }

        public SerializeActivity()
        {
            DisplayName = "Сериализовать объект или коллекцию";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Object, type.ExtractPropertyName(() => Object));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Collection, type.ExtractPropertyName(() => Collection));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Result, type.ExtractPropertyName(() => Result));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var result = new StringBuilder();
            var obj = Object.Get(context);
            var collection = Collection.Get(context);
            if (obj == null && collection == null)
                throw new DeveloperException("Не указан объект для сериализации");
            if(obj != null)
                result.Append(XmlDocumentConverter.ConvertFrom(obj).InnerXml);
            if (collection != null)
            {
                foreach (var c in collection)
                {
                    result.Append(XmlDocumentConverter.ConvertFrom(collection).InnerXml);
                }
            }
            Result.Set(context, result.ToString());
        }
    }
}
