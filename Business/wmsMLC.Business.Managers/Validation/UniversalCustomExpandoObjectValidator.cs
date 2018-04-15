using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.Business.Managers.Validation
{
    /// <summary>
    /// Универсальный валидатор для CustomExpandoObject. По мере необходимости добавляем валидации.
    /// </summary>
    public class UniversalCustomExpandoObjectValidator : AbstractValidatorEngine
    {
        public const string RequirementValidationMethodName = "Requirement";
        public const string PositiveNumberValidationMethodName = "PositiveNumber"; //Положительные числа > 0

        public UniversalCustomExpandoObjectValidator(IValidatable parent)
            : base(parent)
        {
            ValidationMethods = new Dictionary<string, ObjectValid[]>();
        }

        public new CustomExpandoObject ValidatableObject
        {
            get { return (CustomExpandoObject)base.ValidatableObject; }
        }

        /// <summary>
        /// Определяем какое свойство валидировать и как.
        /// </summary>
        public IDictionary<string, ObjectValid[]> ValidationMethods { get; private set; }

        protected override void ValidateProperty_Internal(PropertyDescriptor propertyInfo)
        {
            if (propertyInfo.Name == CustomExpandoObject.MembersPropertyName)
            {
                foreach (var p in ValidatableObject.Members)
                {
                    Validate(p.Key);
                }
            }
            base.ValidateProperty_Internal(propertyInfo);
        }

        protected override void ValidatableObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Validate(e.PropertyName);
            base.ValidatableObject_PropertyChanged(sender, e);
        }

        private void Validate(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || ValidationMethods == null ||
                !ValidationMethods.ContainsKey(propertyName))
                return;

            var methods = ValidationMethods[propertyName];
            if (methods == null || methods.Length == 0)
                return;

            Errors.Remove(propertyName);
            foreach (var m in methods.OrderBy(m => m.ObjectValidPriority))
            {
                switch (m.ObjectValidName)
                {
                    case RequirementValidationMethodName:
                        if (IsNullValueCheck(ValidatableObject.Members[propertyName]))
                        {
                            Errors.Add(propertyName, new ValidateError(m.ObjectValidMessage, m.ObjectValidLevel));
                        }
                        break;
                    case PositiveNumberValidationMethodName:
                        if (!PositiveNumberValidate(ValidatableObject.Members[propertyName]))
                        {
                            Errors.Add(propertyName, new ValidateError(m.ObjectValidMessage, m.ObjectValidLevel));
                        }
                        break;
                }
            }
        }

        private static bool IsNullValueCheck(object value)
        {
            if (value == null)
                return true;

            if ((value is DateTime && value.Equals(DateTime.MinValue)) ||
                (value is Guid && value.Equals(Guid.Empty)) ||
                (value is string && string.IsNullOrEmpty((string)value)))
                return true;

            return false;
        }

        private static bool PositiveNumberValidate(object value)
        {
            if (value == null)
                return false;

            var type = value.GetType().GetNonNullableType();
            if (type.IsPrimitive || type == typeof (decimal))
                return Convert.ToDouble(value) > 0;

            return false;
        }
    }
}
