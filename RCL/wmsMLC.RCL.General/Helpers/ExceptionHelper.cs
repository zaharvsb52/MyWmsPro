using System;
using wmsMLC.General;
using wmsMLC.General.DAL;
using wmsMLC.RCL.Resources;

namespace wmsMLC.RCL.General.Helpers
{
    public static class ExceptionHelper
    {
        public static string GetErrorMessage(Exception ex, bool useErrorUndefined = true)
        {
            var trueEx = GetFirstNotAggregateException(ex);

            trueEx.GetBaseException();

            if (trueEx is TimeoutException)
                return StringResources.Timeout;

            if (trueEx is CommunicationException)
                return StringResources.CommunicationError;

            if (trueEx is ICustomException && !trueEx.Message.IsNullOrEmptyAfterTrim())
                return trueEx.Message;

            if (trueEx.GetBaseException() is DALException && !trueEx.GetBaseException().Message.IsNullOrEmptyAfterTrim())
                return trueEx.GetBaseException().Message;

            if (trueEx is UnauthorizedAccessException && !trueEx.Message.IsNullOrEmptyAfterTrim())
                return trueEx.Message;

            return useErrorUndefined ? StringResources.ErrorUndefined : ex.Message;
        }

        public static Exception GetFirstNotAggregateException(Exception exception)
        {
            if (exception is AggregateException && exception.InnerException != null)
                return GetFirstNotAggregateException(exception.InnerException);

            return exception.InnerException ?? exception;
        }
    }
}
