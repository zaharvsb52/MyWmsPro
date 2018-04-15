using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.General.BL
{
    public class CustomExpandoObject : DynamicObject, IDataErrorInfo, IValidatable, INotifyPropertyChanged
    {
        #region . Fields .

        public const string MembersPropertyName = "Members";

        private Dictionary<string, object> _members =
                new Dictionary<string, object>();
        private Dictionary<string, object> _oldMembers =
                new Dictionary<string, object>();
        private Dictionary<string, object> _initialMembers =
                new Dictionary<string, object>();
        private IValidator _validator;

        #endregion

        #region . Properties .

        public IDictionary<string, object> Members
        {
            get { return _members; }
        }

        #endregion

        #region . Methods .

        public object GetPreviousValue(string name)
        {
            if (!_oldMembers.ContainsKey(name))
                return null;
            return _oldMembers[name];
        }

        public object GetInitialValue(string name)
        {
            if (!_initialMembers.ContainsKey(name))
                return null;
            return _initialMembers[name];
        }

        public void SetInitialValue(string name, object value)
        {
            _initialMembers[name] = value;
        }

        public bool TrySetMember(string name, object value)
        {
            if (!_members.ContainsKey(name))
            {
                _members.Add(name, value);
            }
            else
            {
                // запомним первое значение
                if (!_initialMembers.ContainsKey(name))
                    _initialMembers.Add(name, value);
                // сохраним прошлое значение
                if (!_oldMembers.ContainsKey(name))
                    _oldMembers.Add(name, _members[name]);
                else
                    _oldMembers[name] = _members[name];
                // выставим новое значение
                _members[name] = value;
            }
            OnPropertyChanged(name);
            return true;
        }

        /// <summary>
        /// When a new property is set, 
        /// add the property name and value to the dictionary
        /// </summary>     
        public override bool TrySetMember
             (SetMemberBinder binder, object value)
        {
            return TrySetMember(binder.Name, value);
        }

        /// <summary>
        /// When user accesses something, return the value if we have it
        /// </summary>      
        public override bool TryGetMember
               (GetMemberBinder binder, out object result)
        {
            if (_members.ContainsKey(binder.Name))
            {
                result = _members[binder.Name];
                return true;
            }
            else
            {
                return base.TryGetMember(binder, out result);
            }
        }

        /// <summary>
        /// If a property value is a delegate, invoke it
        /// </summary>     
        public override bool TryInvokeMember
           (InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_members.ContainsKey(binder.Name)
                      && _members[binder.Name] is Delegate)
            {
                result = (_members[binder.Name] as Delegate).DynamicInvoke(args);
                return true;
            }
            else
            {
                return base.TryInvokeMember(binder, args, out result);
            }
        }


        /// <summary>
        /// Return all dynamic member names
        /// </summary>
        /// <returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _members.Keys;
        }

        #endregion

        #region . IDataErrorInfo .

        public string this[string columnName]
        {
            get
            {
                var err = this.Validator.Errors[columnName];
                return err == null
                    ? string.Empty
                    : string.Format("ErrorType={0};ErrorContent={1}", err.GetMaxErrorLevel(), err);
            }
        }

        public string Error
        {
            get { return this.Validator.GetErrorDescription(); }
        }

        #endregion

        #region . IValidatable .

        public void Validate()
        {
            if (Validator != null)
                Validator.Validate();
        }

        [Bindable(false), Browsable(false)]
        public IValidator Validator
        {
            get { return _validator ?? (_validator = CreateValidator()); }
        }

        protected virtual IValidator CreateValidator()
        {
            return new BlankValidator(this);
        }

        public void SetValidator(IValidator validator)
        {
            if (_validator != null)
            {
                _validator.SuspendValidating();
            }
            _validator = validator;
        }

        public void SuspendValidating()
        {
            if (Validator != null)
                Validator.SuspendValidating();
        }

        public void ResumeValidating()
        {
            if (Validator != null)
                Validator.ResumeValidating();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnSourcePropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }
    }
}
