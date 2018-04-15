using System.Windows;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class DXPanelView : PanelView
    {
        public DXPanelView()
        {
            SetBinding(WaitIndicatorVisibleProperty, WaitIndicatorVisibleProperty.Name);
        }

       public bool WaitIndicatorVisible
        {
            get { return (bool) GetValue(WaitIndicatorVisibleProperty); }
            set { SetValue(WaitIndicatorVisibleProperty, value); }
        }

       public static readonly DependencyProperty WaitIndicatorVisibleProperty = DependencyProperty.Register("WaitIndicatorVisible", typeof(bool), typeof(DXPanelView));
    }
}