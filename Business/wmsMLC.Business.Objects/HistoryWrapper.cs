using System;
using System.ComponentModel;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Класс, расшираяющий базовый объект для работы с исторической информацией
    /// </summary>
    /// <typeparam name="T">Расширяемый класс</typeparam>
    [SourceName("HISTORYWRAPPER")]
    public class HistoryWrapper<T> : WMSBusinessObject, ICustomTypeDescriptor, IHistoryItem
    {
        #region .  Consts  .
        public const string HISTORYIDPropertyName = "HISTORYID";
        public const string HDATEFROMPropertyName = "HDATEFROM";
        public const string HDATETILLPropertyName = "HDATETILL";
        public const string ARCHINSTGUID_RPropertyName = "ARCHINSTGUID_R";
        #endregion

        #region .  Methods  .
        protected override CustomPropertyCollection CreateCustomProperties()
        {
            var res = base.CreateCustomProperties();
            res[HISTORYIDPropertyName] = new CustomProperty(HISTORYIDPropertyName, typeof(decimal?), null);
            res[HDATEFROMPropertyName] = new CustomProperty(HDATEFROMPropertyName, typeof(DateTime?), null);
            res[HDATETILLPropertyName] = new CustomProperty(HDATETILLPropertyName, typeof(DateTime?), null);
            res[ARCHINSTGUID_RPropertyName] = new CustomProperty(ARCHINSTGUID_RPropertyName, typeof(Guid?), null);
            return res;
        }

        protected override Type GetEntityType()
        {
            return typeof(T);
        } 
        #endregion

        #region .  ICustomTypeDescriptor  .
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            var items = TypeDescriptor.GetAttributes(typeof(T));
            return items;
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            throw new NotImplementedException();
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            throw new NotImplementedException();
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            throw new NotImplementedException();
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            throw new NotImplementedException();
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            var res = TypeDescriptor.GetProperties(typeof(T));
            return res;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(T), attributes);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}