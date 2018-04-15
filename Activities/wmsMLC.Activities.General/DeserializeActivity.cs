using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.General
{
    public class DeserializeActivity<T> : NativeActivity
    {
        [DisplayName(@"XML")]
        public InArgument<string> Source { get; set; }

        public OutArgument<T> Result { get; set; }

        [DisplayName(@"Новый объект")]
        public bool IsNew { get; set; }

        public DeserializeActivity()
        {
            DisplayName = "Десериализовать объект";
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            var collection = new Collection<RuntimeArgument>();
            var type = GetType();

            ActivityHelpers.AddCacheMetadata(collection, metadata, Source, type.ExtractPropertyName(() => Source));
            ActivityHelpers.AddCacheMetadata(collection, metadata, Result, type.ExtractPropertyName(() => Result));

            metadata.SetArgumentsCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            var source = Source.Get(context);
            if (string.IsNullOrEmpty(source))
                throw new DeveloperException("Нет данных для десериализации");
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(source);
            var result = (T)XmlDocumentConverter.ConvertTo(typeof(T), xmlDoc);
            if (IsNew)
            {
                var editable = result as IEditable;
                if (editable != null)
                    editable.AcceptChanges(true);
            }
            Result.Set(context, result);
        }
    }
}
