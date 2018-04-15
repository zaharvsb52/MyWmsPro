using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using BLToolkit.Mapping;

namespace wmsMLC.General.BL
{
    public abstract class SerializableBusinessObject : BusinessObject, ICustomXmlSerializable
    {
        #region .  Consts  .

        private static readonly Dictionary<string, string> EscapeChars = new Dictionary<string, string>
        {
            {"&gt;", ">"},
            {"&lt;", "<"},
            {"&amp;", "&"},
            {"&apos;", "'"},
            {"&quot;", "\""}
        };

        #endregion

        #region .  ICustomXmlSerializable  .

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
            var properties = TypeDescriptor.GetProperties(GetType()).Cast<DynamicPropertyDescriptor>().ToArray();

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

                // ищем соответствующее свойство (в XML и в метаданных должны быть большие буквы)
                var propertyDesc = properties.FirstOrDefault(i => name.Equals(i.SourceName));
                CustomProperty property;
                if (propertyDesc != null && CustomProperties.ContainsKey(propertyDesc.Name))
                {
                    property = CustomProperties[propertyDesc.Name];
                }
                else
                // свойсва нет - пытаемся присвоить по прочитанному имени (а там в зависимости от настроек)
                {
                    var innerXml = reader.ReadInnerXml();
                    SetProperty(name, innerXml);
                    //reader.Read(); лишнее чтение
                    continue;
                }

                // если объект сам умеет десериализоваться, пусть этим и займется
                if (typeof (IXmlSerializable).IsAssignableFrom(property.PropertyType))
                {
                    // создаем новый экземпляз класса
                    property.SetNewInstance();
                    var obj = ((IXmlSerializable) property.Value);

                    obj.ReadXml(reader);
                }
                else
                {
                    var strValue = reader.ReadElementContentAsString();
                    //HACK: заменяем escape символы на соответсвующие
                    foreach (var esc in EscapeChars)
                        strValue = strValue.Replace(esc.Key, esc.Value);

                    // оставшиеся заносим как есть
                    property.Value = strValue;
                }

                // сообщаем об изменении св-ва
                // ЭТО ОЧЕНЬ ВАЖНО!!! Запускаются механизмы контроля изменений и валидации!
                // TODO: Подумать как убрать эту фигню!!!
                OnPropertyChanged(property.Name);
            }
        }

        private void WriteXmlInternal(XmlWriter writer, bool writeRoot = true)
        {
            if (writeRoot)
                writer.WriteStartElement(SerializationHelper.GetXmlRootName(this).ToUpper());

            var properties = TypeDescriptor.GetProperties(this);
            foreach (var customProperty in CustomProperties)
            {
                var pd = properties.Find(customProperty.Key, true) as DynamicPropertyDescriptor;

                // если нужно игнорить - игнорим
                if (pd != null)
                {
                    if (pd.Attributes[typeof (XmlIgnoreAttribute)] != null)
                        continue;

                    if (((ICustomXmlSerializable) this).IgnoreInnerEntity &&
                        (typeof (IList).IsAssignableFrom(pd.PropertyType) ||
                         typeof (BusinessObject).IsAssignableFrom(pd.PropertyType)) &&
                        (pd.Attributes[typeof (XmlNotIgnoreAttribute)] == null ||
                         ((ICustomXmlSerializable) this).OverrideIgnore == true))
                        continue;
                }

                var elementName = pd == null ? customProperty.Key : pd.SourceName;
                var value = customProperty.Value.Value;

                // если значения нет - сразу закрываем
                if (value == null)
                    writer.WriteElementString(elementName, null);
                else
                {
                    // если объект может сам себя записать - пусть работает
                    var serializable = value as IXmlSerializable;
                    if (serializable != null)
                    {
                        writer.WriteStartElement(elementName);
                        // проверяем может ли "понашенски"
                        var customXmlSerializable = value as ICustomXmlSerializable;
                        if (customXmlSerializable != null)
                        {
                            // NOTE: договрились не писать типы вложенных элементов
                            customXmlSerializable.WriteXml(writer, false);
                        }
                        else
                            serializable.WriteXml(writer);

                        writer.WriteEndElement();
                    }
                    else
                    {
                        var stringElement = SerializationHelper.GetCorrectStringValue(value, pd);

                        writer.WriteElementString(elementName, stringElement);
                    }
                }
            }

            if (writeRoot)
                writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Данный интерфейс необходим для возможности пропуска корневого элемента при сериализации
    /// Такое поведение необхоимо для нормальной работы с вложенными сущностиями
    /// </summary>
    public interface ICustomXmlSerializable : IXmlSerializable
    {
        void WriteXml(XmlWriter writer, bool writeRoot);

        /// <summary>
        /// Если значение равно True, то не сохраняем вложенные сущности, за исключением тех у которых есть атрибут XmlNotIgnoreAttribute.
        /// </summary>
        bool IgnoreInnerEntity { get; set; }

        /// <summary>
        /// Если значение равно True, то не сохраняем вложенные сущности даже при наличии у них атрибута XmlNotIgnoreAttribute.
        /// </summary>
        bool? OverrideIgnore { get; set; }
    }

    //TODO: перенести в General
    public static class SerializationHelper
    {
        public const string DefaultDateTimeStringFormat = "yyyyMMdd HH:mm:ss";
        public const string ShortDateTimeStringFormat = "HH:mm";
        public const string LongDateTimeStringFormat = "yyyyMMdd HH:mm:ss.ffffff"; //20160203 11:54:18.190544
        public const string TrueString = "1";
        public const string FalseString = "0";
        public const string FloatPointSeparator = ".";
        //public static string DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        public const string RootNamePrefix = "TENT";

        public static string DecimalSeparator
        {
            get { return CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator; }
        }

        public static string GetXmlRootName(object entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return GetXmlRootName(entity.GetType());
        }

        public static string GetXmlRootName(Type type)
        {
            //return SourceNameHelper.GetSourceName(this);
            var xmlRootAtt = TypeDescriptor.GetAttributes(type)[typeof (XmlRootAttribute)] as XmlRootAttribute;
            if (xmlRootAtt != null)
                return xmlRootAtt.ElementName;

            // если аттрибута нет - то применяем стандартный шаблон
            return RootNamePrefix + SourceNameHelper.Instance.GetSourceName(type);
        }


        // TODO: вытащить в какой-нибудь ISourceTypeHelper (т.к. данные правила специфичны для каждого источника данных)
        public static string GetCorrectStringValue(object sourceValue)
        {
            if (sourceValue == null)
                return null;

            //HACK: bool нужно передавать через 0/1
            if (sourceValue is bool)
                return (bool) sourceValue ? TrueString : FalseString;

            if (sourceValue is DateTime)
            {
                var value = (DateTime) sourceValue;

                return value.ToString(DefaultDateTimeStringFormat);
            }

            if (sourceValue is Guid)
                return ((Guid) sourceValue).ToString("N").ToUpper();

            // дробные приводим к требуемому формату
            if (sourceValue.GetType().IsFloatType())
                return sourceValue.ToString().Replace(".", FloatPointSeparator).Replace(",", FloatPointSeparator);

            var res = sourceValue.ToString();
            return res;
        }

        internal static string GetCorrectStringValue(object sourceValue, PropertyDescriptor pd)
        {
            if (pd != null && sourceValue is DateTime)
            {
                var attr = pd.Attributes[typeof(DbTypeAttribute)];

                if (attr != null && ((DbTypeAttribute)attr).DBTypeCustom == DbTypeAttribute.DbTypeCustom.TimeStamp)
                    return ((DateTime)sourceValue).ToString(LongDateTimeStringFormat);
            }

            return GetCorrectStringValue(sourceValue);
        }

        public static object ConvertToTrueType(object value, Type trueType)
        {
            // если тип и так соответствует, то возвращаем обратно
            if (value != null && trueType.IsInstanceOfType(value))
                return value;

            var isNullable = trueType.IsNullable();
            if (isNullable && (value == null || Equals(value, string.Empty)))
                return null;

            var notNullableType = trueType.GetNonNullableType();
            if (notNullableType.IsValueType && value == null)
                return Activator.CreateInstance(notNullableType);

            var str = value as string;
            if (str != null)
            {
                if (notNullableType == typeof (bool))
                {
                    switch (str.ToUpper())
                    {
                        case TrueString:
                        case "TRUE":
                            return true;

                        case FalseString:
                        case "FALSE":
                            return false;

                        default:
                            throw new DeveloperException("Can't convert string value to a Boolean");
                    }
                }

                if (notNullableType == typeof (int))
                    return int.Parse(str);

                if (notNullableType.IsEnum)
                    return Enum.Parse(notNullableType, str, true);

                // строку в byte[]
                if (notNullableType == typeof (byte[]))
                {
                    var bytes = new byte[str.Length*sizeof (char)];
                    Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
                    return bytes;
                }

                // дата-время
                if (notNullableType == typeof (DateTime))
                    return DateTime.ParseExact(str,
                        new[] {LongDateTimeStringFormat, DefaultDateTimeStringFormat, ShortDateTimeStringFormat}, null,
                        DateTimeStyles.None);

                // плавающую "точку"
                if (notNullableType == typeof (float))
                    return float.Parse(NormalizeDecimalStr(str));

                if (notNullableType == typeof (double))
                    return double.Parse(NormalizeDecimalStr(str));

                // INFO: здесь может вернуться экспонента
                if (notNullableType == typeof (decimal))
                    return decimal.Parse(NormalizeDecimalStr(str), NumberStyles.Any);
            }

            return TB.ComponentModel.UniversalTypeConverter.Convert(value, notNullableType);
        }

        private static string NormalizeDecimalStr(string str)
        {
            return str.Replace(".", DecimalSeparator).Replace(",", DecimalSeparator);
        }
    }
}