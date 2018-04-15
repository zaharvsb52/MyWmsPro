using System;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    public class DeveloperException : BaseException
    {
        #region  .  Constructors  .
        public DeveloperException() { }
        public DeveloperException(string message) : base(message) { }
        public DeveloperException(string message, Exception innerException) : base(message, innerException) { }
        public DeveloperException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
        #endregion

        public override string Message
        {
            get { return string.Format(StringResources.PrefixDeveloperException, base.Message); }
        }
    }

    public interface IUserMessageException
    {
        string GetUserMessage();
    }
}