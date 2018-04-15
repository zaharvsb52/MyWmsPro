using System;
using System.Windows.Data;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.General.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException(ExceptionResources.TargetMustBeBool);

            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}