using System;

namespace wmsMLC.General.BL.Validation.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
	public class ValidationDependsOnAttribute : Attribute
	{
		public ValidationDependsOnAttribute(string memberName)
		{
			MemberName = memberName;
		}

		public string MemberName { get; private set; }
	}
}
