using System;
using System.Collections.Generic;
using System.ComponentModel;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.General.BL
{
    public class ValidatableObjectCollection<T> : SerializableBusinessObjectCollection<T>, IValidatable
    {
        #region .  Variables  .

        private IValidator _validator;
        private bool _isValidatorInitilized;

        #endregion

        #region .  IValidatable  .

        public ValidatableObjectCollection() { }

        public ValidatableObjectCollection(IEnumerable<T> collection) : base(collection) { }

        public void Validate()
        {
            if (Validator != null)
                Validator.Validate();
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

        [Bindable(false), Browsable(false)]
        public IValidator Validator
        {
            get
            {
                if (_isValidatorInitilized)
                    return _validator;

                _validator = CreateValidator();
                _isValidatorInitilized = true;

                return _validator;
            }
        }

        protected virtual IValidator CreateValidator()
        {
            var factory = IoC.Instance.Resolve<IValidatorFactory>();
            return factory.CreateValidator(this);
        }

        #endregion
    }
}
