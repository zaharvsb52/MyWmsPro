using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Configurator.Views
{
    public sealed class IsDirtyToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.To(false) ? ImageResources.DCLGridRowEdit.GetBitmapImage() : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
