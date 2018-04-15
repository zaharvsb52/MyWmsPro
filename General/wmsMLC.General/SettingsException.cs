using System;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    public class SettingsException : BaseException
    {
        #region  .  Constructors  .
        public SettingsException() { }
        public SettingsException(string message) : base(message) { }
        public SettingsException(string message, Exception innerException) : base(message, innerException) { }
        public SettingsException(string messageFormat, params object[] args) : base(string.Format(messageFormat, args)) { }
        #endregion

        public override string Message
        {
            get { return string.Format(StringResources.PrefixSettingsException, base.Message); }
        }
    }
}