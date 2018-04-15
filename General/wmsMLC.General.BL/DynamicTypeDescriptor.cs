using System;
using System.Linq;
using System.ComponentModel;

namespace wmsMLC.General.BL
{
    public class DynamicTypeDescriptor : ICustomTypeDescriptor
    {
        #region .  Fields  .
        private readonly Type _ownerType;
        private AttributeCollection _attributeCollection;
        private readonly PropertyDescriptorCollection _propertyDescriptorCollection = new PropertyDescriptorCollection(null); 
        #endregion

        public DynamicTypeDescriptor(Type ownerType)
        {
            _ownerType = ownerType;
            _attributeCollection = new AttributeCollection(_ownerType.GetCustomAttributes(true).Cast<Attribute>().ToArray());
        }

        /// <summary> Добавить новое свойство </summary>
        /// <param name="propertyDescriptor">Описатель свойства</param>
        public void AddProperty(DynamicPropertyDescriptor propertyDescriptor)
        {
            _propertyDescriptorCollection.Add(propertyDescriptor);
        }

        #region .  Methods  .
        public AttributeCollection GetAttributes()
        {
            return _attributeCollection;
        }

        public void AddAttribute(Attribute attribute)
        {
            var attributes = _attributeCollection.Cast<Attribute>().ToList();
            attributes.Add(attribute);
            _attributeCollection = new AttributeCollection(attributes.ToArray());
        }

        public void AddAttributes(Attribute[] attributes)
        {
            if (attributes.Length == 0)
                return;
            var existsAttributes = _attributeCollection.Cast<Attribute>().ToList();
            existsAttributes.AddRange(attributes);
            _attributeCollection = new AttributeCollection(existsAttributes.ToArray());
        }

        public string GetClassName()
        {
            throw new NotImplementedException();
        }

        public string GetComponentName()
        {
            throw new NotImplementedException();
        }

        public TypeConverter GetConverter()
        {
            return new TypeConverter();
            //return TypeDescriptor.GetConverter(_ownerType);
        }

        public EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        public object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return _propertyDescriptorCollection;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var props = _propertyDescriptorCollection
                .Cast<PropertyDescriptor>()
                .Where(i => i.Attributes
                             .Cast<Attribute>()
                             .Any(j => Contains(j, attributes)))
                .ToArray();

            return new PropertyDescriptorCollection(props);
        }

        private bool Contains(Attribute at, Attribute[] ats)
        {
            return ats.Any(i => i.Match(at));
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}