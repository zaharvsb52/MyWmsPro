using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;
using DevExpress.Data.Filtering.Helpers;
using System.ComponentModel;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    public sealed class ExpressionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return false;
                var eval = new ExpressionEvaluator(TypeDescriptor.GetProperties(value), parameter == null ? string.Empty : parameter.ToString());
                var result = eval.Fit(value);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public sealed class InternalStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.GetType() == typeof(StyleOption))
                    return true;
                if (value is int)
                {
                    var count = System.Convert.ToInt32(value);
                    return count != 0;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public sealed class ColorToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            if (brush != null)
                return brush.Color;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color)
                return new SolidColorBrush((Color)value);
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public sealed class BooleanRevertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return true;
            bool val;
            if (bool.TryParse(value.ToString(), out val)) return !val;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
}