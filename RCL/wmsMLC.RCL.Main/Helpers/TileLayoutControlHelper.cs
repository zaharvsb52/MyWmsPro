using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.RCL.Main.Helpers
{
    public static class TileLayoutControlHelper
    {
        public static void ScaleTransform(DependencyObject parent, double tileLayoutControlActualWidth, double tileLayoutControlActualHeight)
        {
            var length = Math.Min(tileLayoutControlActualWidth, tileLayoutControlActualHeight) - 4;
            var minlen = (length - 24.0) / 3.0;

            foreach (var tile in VisualTreeHelperExt.FindChildsByType(parent, typeof(CustomTile)).OfType<CustomTile>())
            {
                var scalex = minlen / tile.ActualWidth;
                tile.LayoutTransform = new ScaleTransform(scalex, minlen / tile.ActualHeight);
            }
        }
    }
}
