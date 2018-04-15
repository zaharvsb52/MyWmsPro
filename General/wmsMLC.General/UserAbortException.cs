using System;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    public class UserAbortException : BaseException
    {
        #region  .  Constructors  .
        public UserAbortException() { }
        public UserAbortException(string message) : base(message) { }
        public UserAbortException(string message, Exception innerException) : base(message, innerException) { }
        public UserAbortException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
        #endregion

        public override string Message
        {
            get { return string.Format(StringResources.PrefixUserAbortException, base.Message); }
        }
    }
}
