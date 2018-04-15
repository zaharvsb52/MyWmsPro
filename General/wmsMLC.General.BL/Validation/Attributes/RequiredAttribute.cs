using System;
using System.Collections;
using System.ComponentModel;

namespace wmsMLC.General.BL.Validation.Attributes
{
    public class RequiredAttribute : BaseSimpleValidateAttribute
    {
        #region .  Constatns  .

        public static string DefaultDescription = wmsMLC.General.Resources.StringResources.Validation_RequiredDescription;
        private const ValidateErrorLevel DefaultErrorLevel = ValidateErrorLevel.Warning;

        #endregion

        public RequiredAttribute() : base(DefaultOrdinal, DefaultDescription, DefaultErrorLevel) { }

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
        {
            var value = propInfo.GetValue(obj);
            if (value == null)
            {
                errorsList.Add(new ValidateError(Description, Level));
                return;
            }

            if ((value is DateTime && value.Equals(DateTime.MinValue)) ||
                (value is Guid && value.Equals(Guid.Empty)) ||
                (value is string && string.IsNullOrEmpty((string)value)) ||
                (value is IList && ((IList)value).Count == 0))
                errorsList.Add(new ValidateError(Description, Level));
        }
    }
}
