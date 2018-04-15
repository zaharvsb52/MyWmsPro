using System;

namespace wmsMLC.General
{
    /// <summary>
    /// Exception that need be passed through all layers - till user
    /// </summary>
    public class PassThroughException : BaseException, ICustomException
    {
        #region  .  Constructors  .
        public PassThroughException() { }
        public PassThroughException(string message) : base(message) { }
        public PassThroughException(string message, Exception innerException) : base(message, innerException) { }
        public PassThroughException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
        #endregion
    }
}