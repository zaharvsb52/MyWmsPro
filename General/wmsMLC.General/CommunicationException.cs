using System;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    public class CommunicationException : BaseException
    {
        #region  .  Constructors  .
        public CommunicationException() { }
        public CommunicationException(string message) : base(message) { }
        public CommunicationException(string message, Exception innerException) : base(message, innerException) { }
        #endregion

        public override string Message
        {
            get { return string.Format(StringResources.PrefixCommunicationException, base.Message); }
        }
    }
}
