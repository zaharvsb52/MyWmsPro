using System;

namespace wmsMLC.General.BL.Validation
{
	public class ValidationException : Exception
	{
		#region  .  Constructors  .
		public ValidationException() { }
		public ValidationException(string message) : base(message) { }
		public ValidationException(string message, Exception innerException) : base(message, innerException) { }
		#endregion
	}
}