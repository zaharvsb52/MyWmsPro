using System;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.DAL;

namespace wmsMLC.DCL.General.Helpers
{
    /*
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

            if (trueEx is UnauthorizedAccessException && !trueEx.Message.IsNullOrEmptyAfterTrim())
                return trueEx.Message;

            if (trueEx.GetBaseException() is DALException && !trueEx.GetBaseException().Message.IsNullOrEmptyAfterTrim())
                return trueEx.GetBaseException().Message;

            return useErrorUndefined ? StringResources.ErrorUndefined : ex.Message;
        }

        private static Exception GetFirstNotAggregateException(Exception exception)
        {
            if (exception is AggregateException && exception.InnerException != null)
                return GetFirstNotAggregateException(exception.InnerException);

            return exception.InnerException ?? exception;
        }
    }
    */
}
