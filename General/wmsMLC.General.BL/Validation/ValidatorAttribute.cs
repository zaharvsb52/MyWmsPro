using System;

namespace wmsMLC.General.BL.Validation
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ValidatorAttribute : Attribute
	{
		public ValidatorAttribute(Type validatorType)
		{
			ValidatorType = validatorType;
		}

		public Type ValidatorType { get; private set; }
	}
}
