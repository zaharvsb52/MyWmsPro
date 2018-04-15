using System;
using System.Globalization;
using System.Windows.Data;
using DevExpress.Utils;

namespace wmsMLC.DCL.Main.Helpers
{
    public class BoolToDefaultBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DefaultBoolean.False;
            return (bool) value ? DefaultBoolean.True : DefaultBoolean.False;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            return (DefaultBoolean) value == DefaultBoolean.True;
        }
    }
}
