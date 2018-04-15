using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace wmsMLC.General
{
    public static class XmlDocumentConverter
    {
        #region .  Fields  .
        private static ConcurrentDictionary<Type, Lazy<XmlDocument>> _shortTemplateCache =
            new ConcurrentDictionary<Type, Lazy<XmlDocument>>();

        private static ConcurrentDictionary<Type, Lazy<XmlSerializer>> _serializers =
            new ConcurrentDictionary<Type, Lazy<XmlSerializer>>();
        #endregion

        public static T ConvertTo<T>(XmlDocument xml) where T : class
        {
            return ConvertTo(typeof(T), xml) as T;
        }
        public static T ConvertTo<T>(XmlReader xmlReader) where T : class
        {
            return ConvertTo(typeof(T), xmlReader) as T;
        }

        public static object ConvertTo(Type objectType, XmlDocument xml)
        {
            if (xml == null)
                return null;
            object res = Activator.CreateInstance(objectType);
            FillByXmlDocument(xml, ref res);
            return res;
        }
        public static object ConvertTo(Type objectType, XmlReader reader)
        {
            object res = Activator.CreateInstance(objectType);
            FillByXmlDocument(reader, ref res);
            return res;
        }

        public static void FillByXmlDocument(XmlDocument xml, ref object obj)
        {
            if (xml.DocumentElement == null)
                return;

            using (var reader = new XmlNodeReader(xml))
                FillByXmlDocument(reader, ref obj);
        }

        public static void FillByXmlDocument(XmlReader reader, ref object obj)
        {
            var serialzable = obj as IXmlSerializable;
            if (serialzable == null)
            {
                var ser = GetSerializer(obj.GetType());
                obj = ser.Deserialize(reader);
            }
            else
                serialzable.ReadXml(reader);
        }

        public static List<T> ConvertToListOf<T>(IEnumerable<XmlReader> xmlReaders) where T : class, new()
        {
            return xmlReaders.AsParallel().Select(ConvertTo<T>).ToList();
        }

        public static List<T> ConvertToListOf<T>(IEnumerable<XmlDocument> xmlList) where T : class, new()
        {
            return xmlList.AsParallel().Select(ConvertTo<T>).ToList();
        }
        public static List<XmlDocument> ConvertFromListOf<T>(IEnumerable<T> items) where T : class, new()
        {
            return items.Select(ConvertFrom).ToList();
        }

        public static XmlDocument ConvertFrom(object obj)
        {
            var res = new XmlDocument();

            var serialzable = obj as IXmlSerializable;
            if (serialzable == null)
            {
                using (var stream = new MemoryStream())
                {
                    var ser = GetSerializer(obj.GetType());
                    ser.Serialize(stream, obj);
                    res.LoadXml(Encoding.UTF8.GetString(stream.GetBuffer()));
                    return res;
                }
            }
            
            var dt = obj as DataTable;
            if (dt != null)
            {
                res.LoadXml(dt.DumpToXML());
                return res;
            }
            //TODO: Когда перейдем на .NET 4.5 можно будет использовать settings.DoNotEscapeUriAttributes.
            //      Сейчас при сохранении символов ">" или "<", они будут заменяться на ;gt;lt
            //var settings = new XmlWriterSettings();
            //settings.CheckCharacters = false;
            //settings.DoNotEscapeUriAttributes = false;
            //using (var writer = XmlTextWriter.Create(sb, settings))
            using (var writer = res.CreateNavigator().AppendChild())
            {
                writer.WriteStartDocument();
                serialzable.WriteXml(writer);
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
            return res;
        }

        public static XmlDocument GetShortTemplate(Type type)
        {
            return _shortTemplateCache.GetOrAddSafe(type, CreateShortTemplate);
        }

        private static XmlDocument CreateShortTemplate(Type type)
        {
            var xmlDoc = new XmlDocument();

            //HACK: TENT - нужно куда-нибудь вынести. Когда XmlSerializationHelper переедет в wmsMLC.General - использовать его здесь
            var root = xmlDoc.CreateNode(XmlNodeType.Element,
                string.Format("TENT{0}", SourceNameHelper.Instance.GetSourceName(type).ToUpper()), string.Empty);

            var properties = TypeDescriptor.GetProperties(type);
            foreach (PropertyDescriptor prop in properties)
            {
                var incAtt = prop.Attributes.Cast<Attribute>().Any(i => i.GetType() == typeof (IncludeInPartiallyGetAttribute));
                if (incAtt || !typeof (IXmlSerializable).IsAssignableFrom(prop.PropertyType))
                {
                    var node = xmlDoc.CreateNode(XmlNodeType.Element,
                        SourceNameHelper.Instance.GetPropertySourceName(type, prop.Name), string.Empty);
                    if (incAtt)
                    {
                        foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(prop.PropertyType))
                        {
                            node.AppendChild(xmlDoc.CreateNode(XmlNodeType.Element,
                                SourceNameHelper.Instance.GetPropertySourceName(prop.PropertyType, p.Name), string.Empty));
                            root.AppendChild(node);
                        }
                    }
                    root.AppendChild(node);
                }
            }
            xmlDoc.AppendChild(root);
            return xmlDoc;
        }

        public static XmlDocument XmlDocFromString(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return doc;
        }

        private static XmlSerializer GetSerializer(Type objType)
        {
            return _serializers.GetOrAddSafe(objType, t => new XmlSerializer(t));
        }
    }
}