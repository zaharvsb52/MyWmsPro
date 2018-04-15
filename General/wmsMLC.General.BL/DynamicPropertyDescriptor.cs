using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;

namespace wmsMLC.General.BL
{
    public class DynamicPropertyDescriptor : PropertyDescriptor, ISourceNameHandler
    {
        #region .  Fields  .
        private readonly Type _componentType;
        private readonly Type _propertyType;
        private readonly object _defaultValue;
        private Lazy<string> _sourceNameCache;
        #endregion

        #region .  Constructors  .
        public DynamicPropertyDescriptor(string name, Attribute[] atts, Type componentType, Type propertyType, object defaultValue) : this(name, atts)
        {
            _componentType = componentType;
            _propertyType = propertyType;
            _defaultValue = defaultValue;
        }

        public DynamicPropertyDescriptor(string name, Attribute[] attrs) : base(name, attrs)
        {
            FillSourceCache();
        }

        public DynamicPropertyDescriptor(MemberDescriptor descr)
            : base(descr)
        {
            FillSourceCache();
        }

        public DynamicPropertyDescriptor(MemberDescriptor descr, Attribute[] attrs)
            : base(descr, attrs)
        {
            FillSourceCache();
        }
        #endregion

        private void FillSourceCache()
        {
            _sourceNameCache = new Lazy<string>(() =>
            {
                var sourceAtt = this.Attributes[typeof(SourceNameAttribute)] as SourceNameAttribute;
                return sourceAtt != null ? sourceAtt.SourceName : Name.ToUpper();
            });
        }

        public void AddAttribute(Attribute attribute)
        {
            // вот таким хитрым способом переносим аттрибуты
            var current = new List<Attribute>(this.AttributeArray);
            current.Add(attribute);
            AttributeArray = current.ToArray();
        }

        readonly ConcurrentDictionary<Type, Attribute> _getAttributeByTypeFastCache = new ConcurrentDictionary<Type, Attribute>(); 

        public Attribute GetAttributeByTypeFast(Type type)
        {
            return _getAttributeByTypeFastCache.GetOrAdd(type, t => Attributes[t]);
        }
        
        public T GetAttributeByTypeFast<T>()
            where T : Attribute
        {
            return GetAttributeByTypeFast(typeof(T)) as T;
        }

        #region .  Properties  .
        public override Type ComponentType
        {
            get { return _componentType; }
        }

        public override bool IsReadOnly
        {
            get { return Attributes.Contains(ReadOnlyAttribute.Yes); }
        }

        public override Type PropertyType
        {
            get { return _propertyType; }
        }

        public Object DefaultValue
        {
            get { return _defaultValue; }
        }

        public string SourceName
        {
            get { return _sourceNameCache.Value; }
        }

        #endregion

        #region .  Methods  .
        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object component)
        {
            return ((BusinessObject)component).GetProperty(Name);
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            ((BusinessObject)component).SetProperty(Name, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public interface ISourceNameHandler
    {
        string SourceName { get; }
    }
}