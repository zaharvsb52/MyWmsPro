using System;
using System.Activities.Expressions;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Managers
{
    public static class EntityDescription
    {
        private static readonly List<ProcessAttributeStrategy> _processAttributeStrategies =
            new List<ProcessAttributeStrategy>();

        /// <summary>
        /// Признак необходимости генерации ошибки при прпытке установить неизвестный аттрибут
        /// </summary>
        public static bool UnknownAttibuteThrowExceptionMode { get; set; }

        #region .  Methods  .

        public static DynamicTypeDescriptor GetTypeDescriptor(ISysObjectManager mgr, Type type,
            IEnumerable<SysObject> items)
        {
            var res = new DynamicTypeDescriptor(type);
            foreach (var o in items)
            {
                // разбор
                var attributes = GetAttributes(o,type);

                // описание типа
                var sysObjNameAttr =
                    Attribute.GetCustomAttribute(type, typeof (SysObjectNameAttribute)) as SysObjectNameAttribute;
                var typename = sysObjNameAttr != null
                    ? sysObjNameAttr.SysObjectName
                    : type.Name;

                if (typename.EqIgnoreCase(o.ObjectName))
                {
                    //TODO: правильнее было бы передавать все аттрибуты
                    res.AddAttributes(
                        attributes.Where(p => p is DisplayNameAttribute || p is ListViewCaptionAttribute).ToArray());
                    continue;
                }

                var propertyType = GetObjectTrueType(mgr, o);

                //HACK: Добавляем атрибут CustomXmlIgnoreAttribute к свойству
                if (typeof (IList).IsAssignableFrom(propertyType) && propertyType.IsGenericType)
                {
                    var itemType = propertyType.GetGenericArguments().FirstOrDefault();
                    if (
                        itemType != null && (
                            typeof (AddressBook) == itemType ||
                            typeof (IWBPosQLFDetailDesc) == itemType ||
                            typeof (GlobalParamValue).IsAssignableFrom(itemType) ||
                            typeof (EpsConfig).IsAssignableFrom(itemType)) ||
                        typeof (MotionAreaGroupTr).IsAssignableFrom(itemType)
                        )
                    {
                        var attrs = new List<Attribute>(attributes) {new XmlNotIgnoreAttribute()};
                        attributes = attrs.ToArray();
                    }
                }

                var propertyDesc = new DynamicPropertyDescriptor(o.ObjectName, attributes, type, propertyType,
                    o.ObjectDefaultValue);
                res.AddProperty(propertyDesc);
            }
            return res;
        }

        private static Type GetObjectTrueType(ISysObjectManager mgr, SysObject obj)
        {
            var type = CustomProperty.DefaultType;
            if (obj.ObjectDataType.HasValue)
                type = mgr.GetTypeBySysObjectId(obj.ObjectDataType.Value);

            if (type == null)
                throw new DeveloperException(
                    string.Format(
                        "Для свойства '{0}' объекта '{1}' не удалось определить тип данных '{2}'. Проверьте создан ли объект с данным типом.",
                        obj.ObjectName, obj.ObjectEntityCode, obj.ObjectDataType));

            // если перед нами коллекция - "ну что ж, приступим ..."
            if (obj.ObjectRelationship == Relationship.Many)
                type = typeof (WMSBusinessCollection<>).MakeGenericType(type);
            else
            {
                // Если не указано значение по умолчанию - значит может быть и null
                if (obj.ObjectDefaultValue == null)
                    type = type.GetNullAssignableType();
            }

            return type;
        }

        private static Attribute[] GetAttributes(SysObject sysObject, Type type)
        {
            var attributes = new List<Attribute>();

            // определяем первичный ключ
            if (sysObject.ObjectPK)
                attributes.Add(new PrimaryKeyAttribute());

            if (typeof(Working) == type && sysObject.ObjectDataType.HasValue)
            {
                if (sysObject.ObjectName == "WORKINGTILL" || sysObject.ObjectName == "WORKINGFROM")
                    attributes.Add(new DbTypeAttribute(DbTypeAttribute.DbTypeCustom.TimeStamp));
            }

            // если поле виртуальное - ничего читать, передавать не нужно
            if (string.IsNullOrEmpty(sysObject.ObjectDBName))
                attributes.Add(new XmlIgnoreAttribute());
            else
            {
                // для остальных полей - добавляем стандартные аттрибуты чтения/записи
                attributes.Add(new SourceNameAttribute(sysObject.ObjectDBName));
                attributes.Add(new XmlElementAttribute(sysObject.ObjectDBName));
            }

            // Заполняем LookUP
            if (sysObject.ObjectLookupCode_r != null)
                attributes.Add(new LookupAttribute(sysObject.ObjectLookupCode_r, sysObject.ObjectFieldKeyLink));

            if (sysObject.ObjectExt != null)
            {
                foreach (var objectExt in sysObject.ObjectExt)
                {
                    if (string.IsNullOrEmpty(objectExt.AttrName))
                        throw new DeveloperException("Attribute name can't be null or empty.");

                    // ищем зарегистрированные стратегии
                    var strategies = _processAttributeStrategies
                        .Where(i => i.AttributeName.EqIgnoreCase(objectExt.AttrName))
                        .ToArray();

                    if (UnknownAttibuteThrowExceptionMode && strategies.Length == 0)
                        throw new DeveloperException("Can't find stategy for attribute with name '{0}'.",
                            objectExt.AttrName);

                    // применяем
                    foreach (var strategy in strategies)
                        if (strategy.Processor != null)
                            strategy.Processor(attributes, objectExt.AttrValue);
                }
            }

            // добавляем атрибут, ограничивающий длину поля
            if (sysObject.ObjectFieldLength != null)
                attributes.Add(new MaxLengthAttribute((int) sysObject.ObjectFieldLength));

            return attributes.ToArray();
        }

        public static void RegisterProcessAttributeStrategy(ProcessAttributeStrategy strategy)
        {
            _processAttributeStrategies.Add(strategy);
        }

        #endregion
    }

    public class ProcessAttributeStrategy
    {
        public ProcessAttributeStrategy(string attributeName, Action<List<Attribute>, string> processor)
        {
            AttributeName = attributeName;
            Processor = processor;
        }

        public string AttributeName { get; private set; }
        public Action<List<Attribute>, string> Processor { get; private set; }
    }
}