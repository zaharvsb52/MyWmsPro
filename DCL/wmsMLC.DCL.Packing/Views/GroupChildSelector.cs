using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Input;

namespace wmsMLC.DCL.Packing.Views
{
    public class GroupChildSelector : DependencyObject
    {
        static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached("Mode", typeof(ChildSelectionMode), typeof(GroupChildSelector), new PropertyMetadata(ChildSelectionMode.None, new PropertyChangedCallback(OnModeChanged)));

        public static ChildSelectionMode GetMode(DependencyObject obj)
        {
            return (ChildSelectionMode)obj.GetValue(ModeProperty);
        }
        public static void SetMode(DependencyObject obj, ChildSelectionMode value)
        {
            obj.SetValue(ModeProperty, value);
        }

        static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TableView)) return;
            var view = (d as TableView);
            view.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            view.Grid.GroupRowExpanding += OnGroupRowExpanding;
        }
        static void OnGroupRowExpanding(object sender, RowAllowEventArgs e)
        {
            var view = (e.Source as TableView);
            if (view == null) 
                return;

            view.BeginSelection();
            SelectChild(view, e.RowHandle);
            view.EndSelection();
        }
        static void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var view = (e.Source as TableView);
            if (view == null) 
                return;

            var hitInfo = view.CalcHitInfo(e.OriginalSource as DependencyObject);
            if (!hitInfo.InRow || !view.Grid.IsGroupRowHandle(hitInfo.RowHandle))
                return;

            view.BeginSelection();
            SelectChild(view, hitInfo.RowHandle);
            view.EndSelection();
        }
        static void SelectChild(TableView view, int groupRowHandle)
        {
            var childRowCount = view.Grid.GetChildRowCount(groupRowHandle);
            view.BeginSelection();
            for (var i = 0; i < childRowCount; i++)
            {
                var childRowHandle = view.Grid.GetChildRowHandle(groupRowHandle, i);
                if (GetMode(view) == ChildSelectionMode.Hierarchical && view.Grid.IsGroupRowHandle(childRowHandle) && view.Grid.IsGroupRowExpanded(childRowHandle))
                {
                    SelectChild(view, childRowHandle);
                }
                view.SelectRow(childRowHandle);
            }
            view.EndSelection();
        }
    }

    public enum ChildSelectionMode
    {
        None,
        Child,
        Hierarchical,
    }
}
