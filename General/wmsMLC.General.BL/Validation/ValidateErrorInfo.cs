using System;

namespace wmsMLC.General.BL.Validation
{
	/// <summary>
	/// Класс-обертка (Wrapper) для ValidateError. 
	/// Основная задача - создать связь между BizObjectWithID и ValidateError через ID = RecordID
	/// </summary>
	public class ValidateErrorInfo
	{
		public ValidateErrorInfo(Guid recordID, string attribute, ValidateError error)
		{
			RecordID = recordID;
			Attribute = attribute;
			Error = error;
		}

		public Guid RecordID { get; private set; }

		public string Attribute { get; private set; }

		public ValidateError Error { get; private set; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != typeof (ValidateErrorInfo)) 
				return false;

			return Equals((ValidateErrorInfo) obj);
		}

		public bool Equals(ValidateErrorInfo other)
		{
			if (ReferenceEquals(null, other)) 
				return false;

			if (ReferenceEquals(this, other)) 
				return true;

			return other.RecordID.Equals(RecordID) && 
			       Equals(other.Attribute, Attribute) && 
			       Equals(other.Error, Error);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = RecordID.GetHashCode();
				result = (result*397) ^ (Attribute != null ? Attribute.GetHashCode() : 0);
				result = (result*397) ^ (Error != null ? Error.GetHashCode() : 0);
				return result;
			}
		}
	}
}