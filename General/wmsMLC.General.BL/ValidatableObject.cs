using System.ComponentModel;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.General.BL
{
    public class ValidatableObject : SerializableBusinessObject, IValidatable, IDataErrorInfo
    {
        #region .  Variables  .

        private IValidator _validator;
        private bool _isValidatorInitilized;

        #endregion

        #region .  IValidatable  .

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

        #region .  IDataErrorInfo  .

        public virtual string this[string columnName]
        {
            get
            {
                if (this.Validator == null)
                    return string.Empty;

                var err = this.Validator.Errors[columnName];
                return err == null
                    ? string.Empty
                    : string.Format("ErrorType={0};ErrorContent={1}", err.GetMaxErrorLevel(), err);
            }
        }

        public string Error
        {
            get
            {
                if (this.Validator == null)
                    return null;

                return this.Validator.GetErrorDescription();
            }
        }

        #endregion
    }
}
