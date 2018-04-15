using System;
using System.Collections;
using System.Globalization;
using DevExpress.Data.Filtering;
using log4net;
using wmsMLC.General;

namespace wmsMLC.DCL.Main.Helpers
{
    public class SqlExpressionHelper : ISqlExpressionHelper
    {
        #region .  Fields  .
        private readonly ILog _log = LogManager.GetLogger(typeof(SqlExpressionHelper));
        #endregion

        #region .  Methods  .
        public string GetSqlExpression(string filterExpression)
        {
            try
            {
                if (string.IsNullOrEmpty(filterExpression))
                    return filterExpression;

                if (filterExpression == "()")
                    return string.Empty;

                var op = CriteriaOperator.Parse(filterExpression);
                op = FilterHelperEx.RemoveCriteriaWithNotSetValue(op);
                var whereConverter = new CustomCriteriaToOracleWhereParser();
                var res = whereConverter.Process(op);

                // in (NULL) парсится не корректно
                if (res.Contains("in ()"))
                    return res.Replace("in ()", "in (NULL)");

                // может получиться пустой фильтр
                return res == "()" ? string.Empty : res;
            }
            catch (DevExpress.Data.Filtering.Exceptions.CriteriaParserException)
            {
                _log.WarnFormat("Ошибка определения SQL выражения фильтра {0}", filterExpression);
                throw;
            }
        }

        public bool TryGetSqlExpression(string filterIn, out string filterOut)
        {
            try
            {
                filterOut = GetSqlExpression(filterIn);
                return true;
            }
            catch (Exception)
            {
                filterOut = filterIn;
                return false;
            }
        }

        public string ConstructEqualsWithValue(string fieldName, object value)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            if (value == null)
                return string.Format("{0} is NULL", fieldName);

            if (IsCollection(value))
            {
                var res = string.Empty;
                foreach (var val in (IEnumerable)value)
                {
                    var itemStr = GetCorrectStringValue(val);
                    res += string.IsNullOrEmpty(res) ? string.Empty : ",";
                    res += itemStr;
                }
                return string.Format("{0} in ({1})", fieldName, res);
            }

            var valueStr = GetCorrectStringValue(value);
            return string.Format("{0}={1}", fieldName, valueStr);
        }

        public string ConstructNonEqualsWithValue(string fieldName, object value)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            if (value == null)
                return string.Format("{0} is not NULL", fieldName);

            if (IsCollection(value))
            {
                var res = string.Empty;
                foreach (var val in (IEnumerable)value)
                {
                    var itemStr = GetCorrectStringValue(val);
                    res += string.IsNullOrEmpty(res) ? string.Empty : ",";
                    res += itemStr;
                }
                return string.Format("{0} not in ({1})", fieldName, res);
            }

            var valueStr = GetCorrectStringValue(value);
            return string.Format("{0}<>{1}", fieldName, valueStr);
        }

        public static string GetCorrectStringValue(object value)
        {
            if (value == null)
                return "NULL";

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return "NULL";

                case TypeCode.Boolean:
                    if ((bool)value)
                    {
                        return "1";
                    }
                    return "0";

                case TypeCode.Char:
                    return ("'" + ((char)value) + "'");

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                    if (value is Enum)
                        return Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
                    return value.ToString();

                case TypeCode.UInt64:
                    if (value is Enum)
                        return Convert.ToUInt64(value).ToString(CultureInfo.InvariantCulture);
                    return value.ToString();

                case TypeCode.Single:
                    {
                        float num3 = (float)value;
                        return FixNonFixedText(num3.ToString("r", CultureInfo.InvariantCulture));
                    }
                case TypeCode.Double:
                    {
                        double num2 = (double)value;
                        return FixNonFixedText(num2.ToString("r", CultureInfo.InvariantCulture));
                    }
                case TypeCode.Decimal:
                    {
                        decimal num = (decimal)value;
                        return FixNonFixedText(num.ToString(CultureInfo.InvariantCulture));
                    }
                case TypeCode.DateTime:
                    var time = (DateTime)value;
                    string str = "yyyy/MM/dd";
                    string str2 = "yyyy/mm/dd";
                    if (!(time.TimeOfDay == TimeSpan.Zero))
                    {
                        str = "yyyy/MM/dd:HH:mm:ss";
                        str2 = "yyyy/mm/dd:hh24:mi:ss";
                    }
                    return ("TO_DATE('" + time.ToString(str, CultureInfo.InvariantCulture) + "', '" + str2 + "')");

                case TypeCode.String:
                    return AsString(value);

                default:
                {
                    if (value is Guid)
                        return string.Format("'{0:N}'", value).ToUpper();

                    if (value is TimeSpan)
                    {
                        var span = (TimeSpan) value;
                        return FixNonFixedText(span.TotalSeconds.ToString("r", CultureInfo.InvariantCulture));
                    }

                    throw new ArgumentException(value.ToString());
                }
            }
        }

        private static bool IsCollection(object value)
        {
            return value is IEnumerable && !(value is string);
        }

        private static string FixNonFixedText(string toFix)
        {
            if (toFix.IndexOfAny(new[] { '.', 'e', 'E' }) < 0)
            {
                toFix = toFix + ".0";
            }
            return toFix;
        }

        private static string AsString(object value)
        {
            return ("'" + value.ToString().Replace("'", "''") + "'");
        } 
        #endregion
    }
}