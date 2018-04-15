using System;
using System.Windows.Interactivity;
using DevExpress.Xpf.Docking;
using System.Windows;

namespace wmsMLC.DCL.Main.Helpers
{
    public class DisableFloatingPanelTransparencyBehavior : Behavior<DockLayoutManager>
    {
        private ResourceDictionary floatPaneStyles;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SetValue(DockLayoutManager.AllowFloatGroupTransparencyProperty, false);
            floatPaneStyles = new ResourceDictionary();
            floatPaneStyles.Source = new Uri(@"pack://application:,,,/wmsMLC.DCL.Main;component/themes/floatpane.xaml");
            AssociatedObject.Resources.MergedDictionaries.Add(floatPaneStyles);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Resources.MergedDictionaries.Remove(floatPaneStyles);
            base.OnDetaching();
        }
    }
}
