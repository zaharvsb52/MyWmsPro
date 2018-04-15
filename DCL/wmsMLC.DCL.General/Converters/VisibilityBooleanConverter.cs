using System;
using System.Windows.Data;
using System.Windows;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.General.Converters
{
    public class VisibilityBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException(ExceptionResources.TargetMustBeVisibility);

            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}