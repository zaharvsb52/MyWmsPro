//TODO: хорошо бы переименовать propertyName во что-то более понятное (принимает еще и название типа)
using System;

namespace wmsMLC.General.BL.Validation
{
    public class ValidateEventsArgs : EventArgs
    {
        public ValidateEventsArgs(object sender) : this(sender, null) { }
        public ValidateEventsArgs(object sender, ValidateEventsArgs innerArgs) : this(sender, innerArgs, null) { }
        public ValidateEventsArgs(object sender, ValidateEventsArgs innerArgs, string propertyName)
        {
            Sender = sender;
            InnerArgs = innerArgs;
            PropertyName = propertyName;
        }

        public object Sender { get; private set; }
        public ValidateEventsArgs InnerArgs { get; private set; }
        public string PropertyName { get; private set; }
    }
}
