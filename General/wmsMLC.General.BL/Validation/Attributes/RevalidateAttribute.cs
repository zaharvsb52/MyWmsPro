using System;

namespace wmsMLC.General.BL.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public class RevalidateAttribute : Attribute
	{
		public RevalidateAttribute(string propertyName)
		{
			PropertyName = propertyName;
		}

		public string PropertyName { get; private set; }
	}
}
