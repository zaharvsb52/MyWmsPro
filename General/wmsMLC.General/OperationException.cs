using System;

namespace wmsMLC.General
{
    public class OperationException : BaseException, ICustomException
    {
        #region  .  Constructors  .

        public OperationException(string message) : base(message) { }
        public OperationException(string message, Exception innerException) : base(message, innerException) { }
        public OperationException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }

        #endregion .  Constructors  .
    }
}
