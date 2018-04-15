using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Grid;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.General;

namespace wmsMLC.DCL.Main.Helpers
{
    public sealed class GlyphSizeGeneralToMainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is GlyphSizeType)) 
                return GlyphSize.Default;
            switch ((GlyphSizeType) value)
            {
                case GlyphSizeType.Default:
                    return GlyphSize.Default;
                case GlyphSizeType.Large:
                    return GlyphSize.Large;
                case GlyphSizeType.Small:
                    return GlyphSize.Small;
                default:
                    return GlyphSize.Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridSelectModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return MultiSelectMode.None;

            return (bool)value ? MultiSelectMode.Cell : MultiSelectMode.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GlyphAlignmentGeneralToMainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is GlyphAlignmentType)) 
                return Dock.Left;
            switch (value.To(GlyphAlignmentType.Bottom))
            {
                case GlyphAlignmentType.Bottom:
                    return Dock.Bottom;
                case GlyphAlignmentType.Left:
                    return Dock.Left;
                case GlyphAlignmentType.Right:
                    return Dock.Right;
                case GlyphAlignmentType.Top:
                    return Dock.Top;
                default:
                    return Dock.Left;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DisplayModeTypeGeneralToMainConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DisplayModeType)) 
                return BarItemDisplayMode.Default;
            switch ((DisplayModeType)value)
            {
                case DisplayModeType.Content:
                    return BarItemDisplayMode.Content;
                case DisplayModeType.ContentAndGlyph:
                    return BarItemDisplayMode.ContentAndGlyph;
                case DisplayModeType.Default:
                    return BarItemDisplayMode.Default;
                default:
                    return BarItemDisplayMode.Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
