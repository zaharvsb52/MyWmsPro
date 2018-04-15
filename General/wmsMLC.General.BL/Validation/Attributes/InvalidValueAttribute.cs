using System.ComponentModel;
using System.Linq;

namespace wmsMLC.General.BL.Validation.Attributes
{
    public class InvalidValueAttribute : BaseSimpleValidateAttribute
    {
        #region .  Constants  .

		public const string DefaultDescription = "Значение не может быть равным {0}";
        private const ValidateErrorLevel DefaultErrorLevel = ValidateErrorLevel.Warning;

        #endregion

        #region .  Properties  .

        private object[] Values { get; set; }

        #endregion

        public InvalidValueAttribute(
            object[] value,
			int ordinal = DefaultOrdinal,
			string description = DefaultDescription,
			ValidateErrorLevel level = DefaultErrorLevel)
            : base(ordinal, description, level)
        {
            Values = value;
        }

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
		{			
			var value = propInfo.GetValue(obj);
			if (value == null)
				return;

            if (Values.Any(i => i.Equals(value)))
                errorsList.Add(new ValidateError(string.Format(Description, value), Level));
		}
    }
}
