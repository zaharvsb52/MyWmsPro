using System.Windows;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views
{
    public class DXPanelView : PanelView
    {
        public static readonly DependencyProperty WaitIndicatorVisibleProperty = DependencyProperty.Register("WaitIndicatorVisible", typeof(bool), typeof(DXPanelView));

        public DXPanelView()
        {
            SetBinding(WaitIndicatorVisibleProperty, WaitIndicatorVisibleProperty.Name);
            Helpers.TextOptionsHelper.GetTextOptions(this);
            UseLayoutRounding = true;
            VisualBitmapScalingMode = BitmapScalingMode.NearestNeighbor;
            SnapsToDevicePixels = true;
        }

        public bool WaitIndicatorVisible
        {
            get { return (bool)GetValue(WaitIndicatorVisibleProperty); }
            set { SetValue(WaitIndicatorVisibleProperty, value); }
        }
    }

    public class DXWindowView : DXWindow
    {
    }
}