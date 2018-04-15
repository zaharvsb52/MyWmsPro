using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BLToolkit.Mapping;

namespace wmsMLC.General.BL
{
    public abstract class SerializableBusinessObjectCollection<T> : BusinessObjectCollection<T>, ICustomXmlSerializable
    {
        protected SerializableBusinessObjectCollection() { }

        protected SerializableBusinessObjectCollection(IEnumerable<T> collection) : base(collection) { }

        #region .  IXmlSerializable  .
        XmlSchema IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            var map = this as ISupportMapping;
            try
            {
                if (map != null)
                    map.BeginMapping(null);

                ReadXmlInternal(reader);
            }
            finally
            {
                if (map != null)
                    map.EndMapping(null);
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            WriteXmlInternal(writer);
        }
        #endregion

        #region .  ICustomXmlSerializable  .
        void ICustomXmlSerializable.WriteXml(XmlWriter writer, bool writeRoot)
        {
            WriteXmlInternal(writer, writeRoot);
        }

        bool ICustomXmlSerializable.IgnoreInnerEntity { get; set; }
        bool? ICustomXmlSerializable.OverrideIgnore { get; set; }
        #endregion

        private void ReadXmlInternal(XmlReader reader)
        {
            // открываем чтение, если еще не сделали
            if (reader.ReadState == ReadState.Initial)
                reader.Read();

            // читаем root
            var rootName = reader.Name;
            if (!reader.Read())
                return;

            var instanceCount = 1;
            while (!reader.EOF)
            {
                var name = reader.Name;
                if (name == rootName)
                {
                    // дочитали до конца - выходим
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        instanceCount--;
                        if (instanceCount == 0)
                            break;
                    }
                    instanceCount++;
                }

                // нас интересуют только заполненные элементы
                if (reader.NodeType != XmlNodeType.Element || reader.IsEmptyElement)
                {
                    reader.Read();
                    continue;
                }

                // создаем объект
                if (!typeof(IXmlSerializable).IsAssignableFrom(typeof(T)))
                    throw new DeveloperException("Type {0} is not implement interface IXmlSerializable", typeof(T));

                var obj = (IXmlSerializable)Activator.CreateInstance<T>();
                obj.ReadXml(reader);

                // добавляем в коллекцию
                Add((T)obj);
            }
        }

        private void WriteXmlInternal(XmlWriter writer, bool writeRoot = false)
        {
            if (writeRoot)
                writer.WriteStartElement(SerializationHelper.GetXmlRootName(this).ToUpper());

            foreach (var obj in this)
            {
                var objXmlSer = obj as IXmlSerializable;
                if (objXmlSer == null)
                    throw new DeveloperException("Object {0} is not implement interface IXmlSerializable", obj);

                objXmlSer.WriteXml(writer);
            }

            if (writeRoot)
                writer.WriteEndElement();
        }
    }
}