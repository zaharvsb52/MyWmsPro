using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using BLToolkit.Mapping;
using BLToolkit.Reflection;

namespace wmsMLC.General.BL
{
    public abstract class EditableBusinessObject : ValidatableObject, IEditableObject, ISupportMapping, IEditable, IIsNew
    {
        #region .  Constants  .
        public const string IsDirtyPropertyName = "IsDirty";
        #endregion

        protected EditableBusinessObject()
        {
            IsNew = true;
        }

        #region .  Fields  .
        private bool _isNew;
        private bool _isDirty;
        private bool _isInRejectChanges;
        private Dictionary<string, object> _firstPropertyValues = new Dictionary<string, object>();
        private Dictionary<string, object> _lastPropertyValues = new Dictionary<string, object>();

        private static ConcurrentDictionary<string, Lazy<bool>> _needChangeControlCache
            = new ConcurrentDictionary<string, Lazy<bool>>();
        #endregion

        public bool IsNew
        {
            get { return _isNew; }
            private set
            {
                _isNew = value;
            }
        }

        #region .  IEditableObject  .
        void IEditableObject.BeginEdit()
        {
            //throw new NotImplementedException("Пока не понянтно кто и как будет использовать этот функционал");
            //GetCurrentPropertyValues.Update(_firstPropertyValues);
        }

        void IEditableObject.EndEdit()
        {
            //throw new NotImplementedException("Пока не понянтно кто и как будет использовать этот функционал");
            //SuspendNotifications();
            //AcceptChanges();
        }

        void IEditableObject.CancelEdit()
        {
            //throw new NotImplementedException("Пока не понянтно кто и как будет использовать этот функционал");
            //RejectChanges();
        }
        #endregion

        #region .  ISupportMapping  .
        void ISupportMapping.BeginMapping(InitContext initContext)
        {
            SuspendNotifications();
        }

        void ISupportMapping.EndMapping(InitContext initContext)
        {
            ResumeNotifications();
            AcceptChanges();
        }
        #endregion

        #region .  IEditable  .
        [NotControlChanges]
        public bool IsDirty
        {
            get { return _isDirty; }
            protected set
            {
                if (_isDirty == value)
                    return;

                _isDirty = value;
                OnPropertyChanged(IsDirtyPropertyName);
            }
        }

        public void AcceptChanges(bool isNew = false)
        {
            IsNew = isNew;

            if (!IsDirty)
                return;

            _firstPropertyValues.Clear();
            _lastPropertyValues.Clear();
            foreach (var cp in CustomProperties)
            {
                // если объект сам умеет все - пусть делает
                var editable = cp.Value.Value as IEditable;
                if (editable != null)
                    editable.AcceptChanges();
                else
                    _firstPropertyValues[cp.Key] = cp.Value.Value;
            }

            IsDirty = false;
        }

        public void AcceptChanges(string propertyName)
        {
            if (!IsDirty)
                return;

            var val = GetProperty(propertyName);
            var editable = val as IEditable;
            if (editable != null)
                editable.AcceptChanges(true);
            else
            {
                _firstPropertyValues[propertyName] = val;
                if (_lastPropertyValues.ContainsKey(propertyName))
                    _lastPropertyValues.Remove(propertyName);
            }

            IsDirty = false;
        }

        public void RejectChanges()
        {
            if (!IsDirty)
                return;

            _isInRejectChanges = true;
            try
            {
                // откатываем св-ва
                foreach (var cp in CustomProperties)
                {
                    var editable = cp.Value.Value as IEditable;
                    if (editable != null)
                    {
                        editable.RejectChanges();
                    }
                    else
                    {
                        //Изменено Глебачёвым Ю. А то невозможно работать с BpWorkflow - нельзя сделать RejectChanges
                        if (_firstPropertyValues.ContainsKey(cp.Key))
                            SetProperty(cp.Key, _firstPropertyValues[cp.Key]);
                        _lastPropertyValues.Clear();
                    }
                }
            }
            finally
            {
                _isInRejectChanges = false;
            }
            IsDirty = false;
        }

        [NotControlChanges]
        public bool IsInRejectChanges
        {
            get
            {
                return _isInRejectChanges;
            }
        }

        public bool GetPropertyIsDirty(string propertyName)
        {
            var key = CustomProperties.Keys.FirstOrDefault(i => i.EqIgnoreCase(propertyName));
            if (string.IsNullOrEmpty(key))
                throw new DeveloperException("Параметр '{0}' не найден", propertyName);
            if (!_lastPropertyValues.ContainsKey(key))
                return false;
            return !Equals(_lastPropertyValues[key], CustomProperties[key].Value);
        }

        public string GetDirtyDescription()
        {
            var properties = TypeDescriptor.GetProperties(this);
            var sb = new StringBuilder();
            foreach (PropertyDescriptor property in properties)
            {
                if (!GetPropertyIsDirty(property.Name))
                    continue;

                sb.AppendLine(string.Format("{0} = {1} -> {2}", property.Name, _lastPropertyValues[property.Name], property.GetValue(this)));
            }
            return sb.ToString();
        }

        #endregion

        protected override void OnPropertyChanging(string name, object oldValue, object newValue)
        {
            base.OnPropertyChanging(name, oldValue, newValue);

            if (InSuspendNotifications)
                return;

            if (!_firstPropertyValues.ContainsKey(name))
                _firstPropertyValues[name] = oldValue;
            _lastPropertyValues[name] = oldValue;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (_isInRejectChanges)
                return;

            if (InSuspendNotifications)
                return;

            // пустую строку передают, когда нужно просто уведомить, что объект поменялся
            // для служебных свойств отслеживание не нужно
            if (!string.IsNullOrEmpty(propertyName) && NeedChangeControl(propertyName))
            {
                var property = CustomProperties[propertyName];
                if (property != null && !typeof(EditableBusinessObject).IsAssignableFrom(property.PropertyType))
                {
                    if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        var pdcol = TypeDescriptor.GetProperties(this);
                        var pd = pdcol.Find(propertyName, true);
                        if (pd != null && pd.Attributes[typeof(XmlNotIgnoreAttribute)] == null)
                            return;
                    }

                    IsDirty = true;
                }
            }
        }

        private bool NeedChangeControl(string name)
        {
            return _needChangeControlCache.GetOrAddSafe(name, i =>
                {
                    var p = GetType().GetProperty(name);
                    if (p == null)
                        return true;

                    var ca = p.GetCustomAttributes(typeof(NotControlChangesAttribute), true).FirstOrDefault();
                    if (ca != null)
                        return false;

                    return true;
                });
        }

        protected override void InternalItemPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //if (propertyChangedEventArgs.PropertyName == IsDirtyPropertyName && sender is IList)
            if (sender is IList)
            {
                var pdcol = TypeDescriptor.GetProperties(this);
                var pd = pdcol.Cast<PropertyDescriptor>().SingleOrDefault(p => ReferenceEquals(p.GetValue(this), sender));
                if (pd != null && pd.Attributes[typeof(XmlNotIgnoreAttribute)] == null)
                {
                    base.InternalItemPropertyChanged(sender, propertyChangedEventArgs);
                    return;
                }
            }

            IsDirty = true;
            base.InternalItemPropertyChanged(sender, propertyChangedEventArgs);
        }

        public bool GetUnchangedPropertyValue(string propertyName, out object value)
        {
            value = null;
            if (!_firstPropertyValues.ContainsKey(propertyName))
                return false;

            value = _firstPropertyValues[propertyName];
            return true;
        }

        public bool GetPreviosPropertyValue(string propertyName, out object value)
        {
            value = null;
            if (!_lastPropertyValues.ContainsKey(propertyName))
                return false;

            value = _lastPropertyValues[propertyName];
            return true;
        }
    }

    public interface IIsNew
    {
        bool IsNew { get; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public sealed class NotControlChangesAttribute : Attribute
    {
    }
}