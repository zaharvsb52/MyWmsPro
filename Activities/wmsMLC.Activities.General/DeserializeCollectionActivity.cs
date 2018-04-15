using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Activities.General
{
    public class DeserializeCollectionActivity<T> : NativeActivity
    {
        [DisplayName(@"XML")]
        public InArgument<string> Source { get; set; }

        public OutArgument<List<T>> Result { get; set; }

        [DisplayName(@"Новый объект")]
        public bool IsNew { get; set; }

        public DeserializeCollectionActivity()
        {
            DisplayName = "Десериализовать коллекцию";
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
            if (xmlDoc.DocumentElement == null)
                throw new DeveloperException("Не верный формат исходных данных");
            var result = new List<T>();
            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                var doc = new XmlDocument();
                doc.LoadXml(node.OuterXml);
                var obj = (T)XmlDocumentConverter.ConvertTo(typeof(T), doc);
                if (IsNew)
                {
                    var editable = obj as IEditable;
                    if (editable != null)
                        editable.AcceptChanges(true);
                }
                result.Add(obj);
            }
            Result.Set(context, result);
        }
    }
}
