using System;
using System.Xml;
using System.Xml.Serialization;

namespace wmsMLC.Business.General
{
    [XmlRoot("WMSTELEGRAM")]
    public class ImportObject
    {
        /// <summary>
        /// Уникальный идентификатор клиента (GUID)
        /// </summary>
        [XmlElement(ElementName = "ID", IsNullable = false)]
        public Guid Id;

        /// <summary>
        /// Дата отправки телеграммы
        /// </summary>
        [XmlElement(ElementName = "DATE", IsNullable = false)]
        public string Date;

        /// <summary>
        /// Тип телеграммы
        /// </summary>
        [XmlElement(ElementName = "TYPE", IsNullable = false)]
        public TelType Type;

        /// <summary>
        /// Имя сущности
        /// </summary>
        [XmlElement(ElementName = "ENTITY", IsNullable = true)]
        public string Entity;

        /// <summary>
        /// Код телеграммы
        /// </summary>
        [XmlElement(ElementName = "TELCODE", IsNullable = false)]
        public TelCodeEnum TelCode;

        /// <summary>
        /// Комманда API/Название процесса
        /// </summary>
        [XmlElement(ElementName = "ACTION", IsNullable = false)]
        public string Action;
        
        /// <summary>
        /// Расположение склада
        /// </summary>
        [XmlElement(ElementName = "SITE", IsNullable = true)]
        public string Site;

        /// <summary>
        /// Название склада
        /// </summary>
        [XmlElement(ElementName = "WAREHOUSE", IsNullable = true)]
        public string Warehouse;
        
        /// <summary>
        /// Объекты телеграммы
        /// </summary>
        [XmlElement(ElementName = "CONTENT", IsNullable = false)]
        public ObjectContext Content;
    }

    public class ObjectContext
    {
        [XmlAnyElement]
        public XmlNode[] Items;
    }

    public enum TelCodeEnum
    {
        NONE, WMS_API, WMS_PROCESS, WMS_INSERT
    }

    public enum TelType
    {
        NONE, QUERY, ANSWER, ACTION
    }
}
