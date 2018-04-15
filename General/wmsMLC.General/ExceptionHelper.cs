using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text.RegularExpressions;
using wmsMLC.General.Resources;

namespace wmsMLC.General
{
    public static class ExceptionHelper
    {
        public const string OraTemplate = "ORA-";
        public const int OraPrefLength = 11;
        public const int OraUserDefaultErrorNumber = 20000;

        /// <summary> Формирует сообщение, которое "не стыдно" показать пользователю </summary>
        public static string GetErrorMessage(Exception ex, bool useErrorUndefined = true)
        {
            var trueEx = GetFirstMeaningException(ex);
            if (trueEx == null)
                return ExceptionResources.Undefined;

            if (trueEx is TimeoutException)
                return ExceptionResources.TimeoutExceptionMessage;

            if (trueEx is CommunicationException || trueEx is System.ServiceModel.EndpointNotFoundException)
                return ExceptionResources.ServiceUnavailable;

            if (trueEx is ICustomException ||
                trueEx is UnauthorizedAccessException)
                return trueEx.Message;

            var webFaultException = trueEx as WebFaultException;
            if (webFaultException != null && webFaultException.StatusCode == HttpStatusCode.BadRequest)
                return ExceptionResources.BadRequestException;

            return useErrorUndefined ? ExceptionResources.Undefined : trueEx.Message;
        }

        /// <summary>
        /// Преобразование исключения в строку. Использование этого метода обеспечит единый формат вывода
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <returns>Строковое описание исключения</returns>
        public static string ExceptionToString(Exception ex)
        {
            return ExceptionToString(ex, 0);
        }

        internal static string ExceptionToString(Exception ex, int ident)
        {
            if (ex == null)
                return string.Empty;

            var pref = ident == 0 ? string.Empty : new string('-', ident) + ">";

            var message = ex.Message;
            var aggEx = ex as AggregateException;
            if (aggEx != null)
            {
                message = string.Empty;
                // если нет пояснений, выведем, что есть
                if (aggEx.InnerExceptions == null || aggEx.InnerExceptions.Count == 0)
                    return pref + aggEx.Message;

                // в общем случае может быть более одной ошибки
                foreach (var item in aggEx.InnerExceptions)
                {
                    message += string.Format("{0}{1}{2}", string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine, pref, ExceptionToString(item, ident));
                }

                return message;
            }

            if (ex.InnerException != null)
            {
                message += string.Format("{0}{1}{2}", string.IsNullOrEmpty(message) ? string.Empty : Environment.NewLine, pref, ExceptionToString(ex.InnerException, ident + 1));
            }

            return message;
        }

        public static Exception GetFirstMeaningException(Exception ex)
        {
            if (ex == null)
                return null;

            if (ex is AggregateException ||
                ex is TargetInvocationException ||
                ex is BLToolkit.Data.DataException)
                return GetFirstMeaningException(ex.InnerException);

            //Ищем WebFaultException
            var webFaultException = GetFirstExceptionByType(ex, typeof (WebFaultException));
            if (webFaultException != null)
                return webFaultException;

            return ex;
        }

        public static Exception GetFirstNotAggregateException(Exception exception)
        {
            if (exception is AggregateException && exception.InnerException != null)
                return GetFirstNotAggregateException(exception.InnerException);

            return exception.InnerException ?? exception;
        }

        public static Exception GetFirstExceptionByType(Exception exception, Type exceptionType)
        {
            if (exception == null || exceptionType == null)
                return null;

            if (exception.GetType() == exceptionType)
                return exception;

            if (exception.InnerException != null)
                return GetFirstExceptionByType(exception.InnerException, exceptionType);

            return null;
        }

        public static Exception ProcessDalException(Exception ex)
        {
            if (ex == null)
                return null;

            var newEx = GetFirstMeaningException(ex);
            if (newEx is ICustomException)
                return newEx;


            //Получаем список ORA-
            var message = newEx.Message;
            //var expressions = Regex.Matches(message2, @".*?ORA-\d*:\s*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            //var expressions = Regex.Matches(message2, @".*?ORA-\d*:\s*([^,]*).*$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            var expressions = Regex.Matches(message, @".*?ORA-\d*:\s*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var match = expressions.Cast<Match>().ToArray();
            if (match.Length == 0)
                return newEx;

            //Ищем номер ошибки больше OraUserDefaultErrorNumber
            var oralength = OraTemplate.Length;
            foreach (var m in match)
            {
                var ora = m.Value.Trim();
                int number;
                if (ora.Length > oralength && int.TryParse(ora.Substring(oralength, ora.Length - oralength - 1), out number))
                {
                    if (number >= OraUserDefaultErrorNumber)
                    {
                        //Ищем это сообщение
                        var index = message.IndexOf(m.Value, StringComparison.Ordinal);
                        if (index < 0)
                            break;

                        message = message.Remove(0, index);

                        // убираем начало ошибки
                        message = message.Remove(0, OraPrefLength);

                        // выбираем только одну строку
                        index = message.IndexOf(OraTemplate, StringComparison.Ordinal);
                        var errmessage = index >= 0 ? message.Substring(0, index) : message;
                        var errmessageLength = errmessage.Length;
                        while (errmessageLength > 1 && (errmessage.EndsWith("\r") || errmessage.EndsWith("\n")))
                        {
                            errmessage = errmessage.Substring(0, --errmessageLength);
                        }
                        return new OperationException(errmessage);
                    }
                }
            }

            return newEx;
        }
    }
}