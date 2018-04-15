using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Enums;

namespace wmsMLC.General.PL.WPF.Converters
{
    public sealed class StringToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is DateTime)
                return value;

            if (value is string)
            {
                var formats = parameter as string[];
                if (formats != null && formats.Length > 0)
                {
                    var thrculture = System.Threading.Thread.CurrentThread.CurrentCulture;
                    var cultureinternal = culture ?? thrculture;

                    try
                    {
                        return DateTime.ParseExact(value.ToString(), formats, cultureinternal, DateTimeStyles.None);
                    }
                    catch
                    {
                        try
                        {
                            return DateTime.ParseExact(value.ToString(), formats, thrculture, DateTimeStyles.None);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is DateTime)
            {
                var formats = parameter as string[];
                if (formats != null && formats.Length > 0)
                    return System.Convert.ToDateTime(value).ToString(formats[0]);
            }

            return null;
        }
    }

    public sealed class StringToNumericConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(parameter is Type))
                return null;

            return SerializationHelper.ConvertToTrueType(value, (Type)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : SerializationHelper.ConvertToTrueType(value, typeof(string));
        }

        //private CultureInfo CreateCulture(string separator)
        //{
        //    var result = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.LCID);
        //    var numberFormat = result.NumberFormat;
        //    numberFormat.CurrencyDecimalSeparator = numberFormat.NumberDecimalSeparator = separator;
        //    return result;
        //}
    }

    /// <summary>
    /// Конвертирует тип bool в 0 или 1.
    /// </summary>
    public sealed class StringToDbBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return SerializationHelper.ConvertToTrueType(value, typeof(bool));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool ? System.Convert.ToInt32(value) : (int?) null;
        }
    }

    public sealed class ListItemStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ListItemStyle))
                return value;

            var style = (ListItemStyle)value;
            if (style == ListItemStyle.None)
                return value;

            if (targetType == typeof(FontWeight))
            {
                if ((style & ListItemStyle.FontWeightBold) == ListItemStyle.FontWeightBold)
                {
                    return FontWeights.Bold;
                }
            }
            if (targetType == typeof(Brush))
            {
                switch (parameter.To(string.Empty).ToLower())
                {
                    case "foreground":
                        if ((style & ListItemStyle.ForegroundRed) == ListItemStyle.ForegroundRed)
                            return new SolidColorBrush(Colors.Red);
                        if ((style & ListItemStyle.ForegroundWhite) == ListItemStyle.ForegroundWhite)
                            return new SolidColorBrush(Colors.White);
                        break;
                    case "background":
                        if ((style & ListItemStyle.BackgroundRed) == ListItemStyle.BackgroundRed)
                            return new SolidColorBrush(Colors.Red);
                        break;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертирует значение для физических величин .
    /// </summary>
    public class ConverterSI : IValueConverter, IConverterSI
    {
        private string _paramTo;
        private string _paramFrom;
        private int _delta;
     
        public string ParamToFormat { get; private set; }
        public ModeConverter Mode { get; set; }
        public string ParamTo
        {
            get { return _paramTo; }
            set
            {
                _paramTo = value;
                ParamToFormat = GetText(_paramTo);
            }
        }
        public string ParamFrom
        {
            get { return _paramFrom; }
            set
            {
                _paramFrom = value;
                _delta = CalcDelta(_paramFrom);
            }
        }
      
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertTo(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertTo(value, parameter, true);
        }

        public object ConvertTo(object value, object parameter, bool back = false)
        {
            if (value == null)
                return null;

            if (_delta <= 0)
                return value;
            
            if (ModeConverter.Format.Equals(Mode) || ModeConverter.None.Equals(Mode) )
                return ModeConverter.None.Equals(Mode) ? value : string.Format("{0} {1}", value, ParamToFormat);

            try
            {
                var val = System.Convert.ToDecimal(value);

                switch (ParamTo)
                {
                    case "km":
                    case "t":
                    val = back ? (val * 1000000 * _delta) : (val / (1000000 * _delta));
                        break;
                    case "m":
                    case "kg":
                        val = back ? (val*1000*_delta) : (val/(1000*_delta));
                        break;
                    case "cm":
                        val = back ? (val*10*_delta) : (val/(10*_delta));
                        break;
                    case "mm":
                    case "g":
                        val = back ? (val*_delta) : (val/(_delta));
                        break;
                    case "mg":
                        val = back ? (val / (1000 * _delta)) : (val * 1000 * _delta);
                        break;
                    default:
                        return val;
                }

                return ModeConverter.Converter.Equals(Mode) ? val : (object)string.Format("{0} {1}", val, ParamToFormat);
            }
            catch
            {
                return value;
            }
        }

        private string GetText(string param)
        {
            switch (param)
            {
                case "km":
                    return "км";
                case "m":
                    return "м";
                case "cm":
                    return "cм";
                case "mm":
                    return "мм";
                case "t":
                    return "т";
                case "kg":
                    return "кг";
                case "g":
                    return "гр";
                case "mg":
                    return "мг";
                default:
                    return string.Empty;
            }
        }

        private int CalcDelta(string param)
        {
            switch (param)
            {
                case "km":
                case "t":
                    return 1 / 1000000;
                case "m":
                case "kg":
                    return 1 / 1000;
                case "cm":
                    return 1 / 10;
                case "mm":
                case "g":
                    return 1;
                case "mg":
                    return 100;
                default:
                    return 1;
            }
        }
    }

    public interface IConverterSI
    {
        string ParamTo { get; set; }
        string ParamToFormat { get; }
        string ParamFrom { get; set; }
        ModeConverter Mode { get; set; }
    }


    public enum ModeConverter
    {
        None,
        FormatAndConverter,
        Format,
        Converter
    }
}
