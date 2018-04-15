using System;
using System.Dynamic;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace wmsMLC.General.BL
{
    /// <summary>
    /// Базовый класс для динамических объектов
    /// </summary>
    public abstract class BusinessObject : DynamicObject, INotifyPropertyChanged, IRaisePropertyChanged
    {
        #region .  Fields  .
        private CustomPropertyCollection _customProperties;
        private readonly object _hardcodedGetLock = new object();
        private volatile static PropertyInfo[] _hardCodedProperties;
        #endregion

        #region .  Properties  .
        internal CustomPropertyCollection CustomProperties
        {
            get
            {
                return _customProperties ?? (_customProperties = CreateCustomProperties());
            }
        }

        /// <summary>
        /// Режим обработки неизвестных свойств
        /// </summary>
        protected UnknownPropertySetMode UnknownPropertySet { get; set; }

        /// <summary>
        /// Признак того, что объект находится в режиме подавления событий об изменении
        /// </summary>
        protected bool InSuspendNotifications { get; private set; }

        #endregion

        #region .  Methods  .

        public dynamic AsDynamic()
        {
            return this;
        }

        // Динамизация используется только пока тесты не переведены на кастомный динамический объект
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            try
            {
                result = GetProperty(binder.Name);
                return true;
            }
            catch
            {
                return false;
            }
        }
        // Динамизация используется только пока тесты не переведены на кастомный динамический объект
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                SetProperty(binder.Name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return CustomProperties.Keys;
        }

        public T GetProperty<T>(string name)
        {
            var value = GetProperty(name);
            if (value == null)
                return default(T);
            return (T)value;
        }

        public virtual object GetProperty(string name)
        {
            // ищем среди динамических
            var property = CustomProperties[name];
            if (property != null)
                return property.Value;

            // ищем среди статических
            var pi = GetType().GetProperty(name);
            if (pi != null)
                return pi.GetValue(this, null);

            // если и тут не нашли - ругаемся
            throw new DeveloperException("Can't find property " + name);
        }

        public void SetProperty(string name, object value)
        {
            var property = CustomProperties[name];// .FirstOrDefault(i => name.EqIgnoreCase(i.Name));
            if (property == null)
            {
                switch (UnknownPropertySet)
                {
                    case UnknownPropertySetMode.Ignore:
                        return;

                    case UnknownPropertySetMode.ThrowException:
                        throw new DeveloperException("Try to set unknown property {0}. Check name or change UnknownPropertySet mode", name);

                    case UnknownPropertySetMode.AddNewProperty:
                        property = new CustomProperty(name);
                        CustomProperties[name] = property;
                        break;

                    default:
                        throw new DeveloperException("Unknown UnknownPropertySetMode");
                }
            }
            
            // TODO: проверить!!!
            if (Equals(property.Value, value))
                return;

            OnPropertyChanging(name, property.Value, value);
            property.Value = value;
            OnPropertyChanged(name);
        }

        public bool ContainsProperty(string name)
        {
            return CustomProperties.ContainsKey(name);
        }


        /// <summary>
        /// Создание св-в объекта
        /// <Note>Базовый функционал создает коллецию св-в отмеченных аттрибутом Hadcoded</Note>
        /// </summary>
        /// <returns>Коллекцию св-в объекта</returns>
        protected virtual CustomPropertyCollection CreateCustomProperties()
        {
            // создаем коллекцию свойств вручную, т.к. не можем вычитать себя же из базы
            var cln = new CustomPropertyCollection();
            foreach (var p in GetHardCodedProperties())
            {
                var prop = new CustomProperty(p.Name,
                                              p.PropertyType,
                                              p.PropertyType.IsValueType
                                                  ? Activator.CreateInstance(p.PropertyType)
                                                  : null);
                cln[p.Name] = prop;
            }
            return cln;
        }

        private IEnumerable<PropertyInfo> GetHardCodedProperties()
        {
            if (_hardCodedProperties != null)
                return _hardCodedProperties;

            lock (_hardcodedGetLock)
            {
                if (_hardCodedProperties != null)
                    return _hardCodedProperties;

                _hardCodedProperties = GetType()
                    .GetProperties()
                    .Where(i => i.GetCustomAttributes(typeof(HardCodedPropertyAttribute), true).Length > 0)
                    .ToArray();
            }

            return _hardCodedProperties;
        }

        #endregion

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanging(string name, object oldValue, object newValue) { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                var property = GetProperty(propertyName);
                if (property is INotifyPropertyChanged)
                    SubscribeOnItemPropertyChanged((INotifyPropertyChanged)property);
            }

            if (InSuspendNotifications)
                return;

            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubscribeOnItemPropertyChanged(INotifyPropertyChanged sender)
        {
            sender.PropertyChanged -= InternalItemPropertyChanged;
            sender.PropertyChanged += InternalItemPropertyChanged;
        }

        protected virtual void InternalItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) { }

        public void SuspendNotifications()
        {
            InSuspendNotifications = true;
        }

        public void ResumeNotifications()
        {
            InSuspendNotifications = false;
            OnPropertyChanged(string.Empty);
        }
        #endregion

        void IRaisePropertyChanged.RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        public T GetPropertyDefaultValue<T>(string name)
        {
            var value = GetPropertyDefaultValue(name);
            if (value == null)
                return default(T);
            return (T)value;
        }
        public virtual object GetPropertyDefaultValue(string name)
        {
            var property = CustomProperties[name];
            if (property != null)
                return property.DefaultValue;
            return null;
        }
    }

    /// <summary>
    /// Аттрибут, которым должны быть помечены все свойсва, вынесенные в объект (HardCoded)
    /// </summary>
    public class HardCodedPropertyAttribute : Attribute
    {
        public HardCodedPropertyAttribute() { }
        
        /// <param name="propertyName">Имя св-ва в объекте (если оно не совпадает с именем в классе)</param>
        public HardCodedPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; private set; }
    }

    /// <summary>
    /// Данный атрибут может быть использован для переопределения ObjectName класса.
    /// Область применения: различные наследники классов, которые должны получать св-ва родителей
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SysObjectNameAttribute : Attribute
    {
        public SysObjectNameAttribute(string objectName)
        {
            SysObjectName = objectName;
        }

        public string SysObjectName { get; private set; }
    }

}