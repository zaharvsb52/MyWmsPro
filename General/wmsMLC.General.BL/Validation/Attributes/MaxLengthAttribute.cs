using System.ComponentModel;

namespace wmsMLC.General.BL.Validation.Attributes
{
    public class MaxLengthAttribute : BaseSimpleValidateAttribute
    {
        #region .  Constants  .

        public const string DefaultDescription = "Превышено максимальное кол-во элементов";
        private const ValidateErrorLevel DefaultErrorLevel = ValidateErrorLevel.Critical;

        #endregion

        public int MaxLength { get; private set; }

        public MaxLengthAttribute(int maxLength, int ordinal = DefaultOrdinal, string description = DefaultDescription, ValidateErrorLevel level = DefaultErrorLevel) : base(ordinal, description, level)
        {
            MaxLength = maxLength;
        }

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
        {
            var value = propInfo.GetValue(obj);
            if (value == null)
                return;

            if (value.ToString().Length > MaxLength)
                errorsList.Add(new ValidateError(Description, Level));
        }
    }
}