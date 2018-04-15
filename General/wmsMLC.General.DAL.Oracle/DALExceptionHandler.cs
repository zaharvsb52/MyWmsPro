using System;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Oracle.DataAccess.Client;

namespace wmsMLC.General.DAL.Oracle
{
    public static class DALExceptionHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DALExceptionHandler));

        public static Exception ProcessException(Exception ex, bool allowLog = true)
        {
            // переопределяем
            var newEx = ExceptionHelper.GetFirstMeaningException(ex);

            // уже обработали
            if (newEx is DALException)
                return newEx;

            // разбираем
            var oraException = newEx as OracleException;
            if (oraException != null)
                newEx = new DALCustomException(GetDalExceptionMessage(oraException), newEx);

            // логируем исходную
            if (allowLog)
            {
                var message = ExceptionHelper.GetErrorMessage(ex, false);
                if (newEx is ICustomException)
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn(message);
                }
                else
                {
                    if (Log.IsErrorEnabled)
                        Log.Error(message, ex);
                }
            }

            // отдаем
            return newEx;
        }

        private static string GetDalExceptionMessage(OracleException ex)
        {
            switch (ex.Number)
            {
                // system
                case 1: return ExceptionResources.DuplicateKeyError;
                case 1400: return string.Format(ExceptionResources.FieldIsNullError, GetField(ex.Message));

                // user
                case 20002: return ExceptionResources.ForeignKeyConstraintError;

                default:
                    if (ex.Number < ExceptionHelper.OraUserDefaultErrorNumber)
                        return ExceptionResources.CommonDataError;

                    // убираем начало ошибки
                    var message = ex.Message.Remove(0, ExceptionHelper.OraPrefLength);

                    // выбираем только одну строку
                    var index = message.IndexOf(ExceptionHelper.OraTemplate, StringComparison.Ordinal);
                    return index >= 0 ? message.Substring(0, index) : message;
            }
        }

        private static string GetField(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;
            var expressions = Regex.Matches(message, @"\(\"".+\""\.\"".+\""\.\""(.+)\""\)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var match = expressions.Cast<Match>().FirstOrDefault();
            string result = null;
            if (match != null && match.Groups.Count > 1)
                result = match.Groups[1].Value;

            return result;
        }
    }
}