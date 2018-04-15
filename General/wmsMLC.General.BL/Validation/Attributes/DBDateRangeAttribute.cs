using System;
using System.ComponentModel;
using wmsMLC.General.Resources;

namespace wmsMLC.General.BL.Validation.Attributes
{
	public class DBDateRangeAttribute : BaseSimpleValidateAttribute
	{
		#region .  Variables & Constants  .

		public const string DefaultDescription = "Дата должна быть между 01 янв 1753 и 31 дек 9999";
		public static readonly DateTime MinDate = new DateTime(1753, 1, 1);
		public static readonly DateTime MaxDate = new DateTime(9999, 12, 31);
		private const ValidateErrorLevel DefaultErrorLevel = ValidateErrorLevel.Critical;

		#endregion

		public DBDateRangeAttribute(
			int ordinal = DefaultOrdinal,
			string description = DefaultDescription,
			ValidateErrorLevel level = DefaultErrorLevel)
			: base(ordinal, description, level) { }

        public override void Validate(object obj, PropertyDescriptor propInfo, ValidateErrorsList errorsList)
		{
			if (propInfo.PropertyType != typeof (DateTime?) && propInfo.PropertyType != typeof (DateTime))
                throw new DeveloperException(string.Format(DeveloperExceptionResources.IncorrectUseOfAttribute, "DBDateRangeAttribute", "DateTime"));

			var value = propInfo.GetValue(obj);
			if (value == null)
				return;

			if ((DateTime)value >= MinDate && (DateTime)value <= MaxDate) 
				return;

			errorsList.Add(new ValidateError(Description, Level));
		}
	}
}
