using System;
using System.Windows.Data;

namespace wmsMLC.DCL.Main.Helpers
{
    public class ErrorContentConverter : IValueConverter
    {
        public string GetValueTag { get; set; }
        public string Separator { get; set; }

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is string))
                return value;
            string error = System.Convert.ToString(value, culture);
            if (string.IsNullOrEmpty(error))
                return value;

            string searchString = GetValueTag + "=";
            foreach (string suberror in error.Split(new string[] { Separator }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (suberror.Contains(searchString))
                    return suberror.Replace(searchString, string.Empty);
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}