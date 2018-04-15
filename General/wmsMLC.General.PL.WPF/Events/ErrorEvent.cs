using System;

namespace wmsMLC.General.PL.WPF.Events
{
    public class ErrorEvent
    {
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        public ErrorEvent(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }
}