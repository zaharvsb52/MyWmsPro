using System;
using System.ComponentModel;
using wmsMLC.General.Resources;

namespace wmsMLC.General.BL.Validation.Attributes
{
	public class DateFutureAttribute : BaseSimpleValidateAttribute
	{
		#region .  Constants  .

		public const string DefaultDescription = "Не может быть в будущем";
        private const ValidateErrorLevel DefaultErrorLevel = ValidateErrorLevel.Warning;

		#endregion

		public DateFutureAttribute(int ordinal = DefaultOrdinal, string description = DefaultDescription, ValidateErrorLevel level = DefaultErrorLevel) : base(ordinal, description, level) { }

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
		{
			if (propInfo.PropertyType != typeof(DateTime) && propInfo.PropertyType != typeof(DateTime?))
				throw new DeveloperException(string.Format(DeveloperExceptionResources.IncorrectUseOfAttribute, "DateFutureAttribute", "DateTime"));

			var value = propInfo.GetValue(obj);
			if (value == null)
				return;

			if (((DateTime)value) > DateTime.Now)
				errorsList.Add(new ValidateError(Description, Level));
		}
	}
}
