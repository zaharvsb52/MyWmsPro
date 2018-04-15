using wmsMLC.DCL.Main.ViewModels;

namespace wmsMLC.DCL.Main.Views
{
    public partial class PropertyView
    {
        public PropertyView()
        {
            InitializeComponent();
        }

        private void PropertyGridControl_CellValueChanged(object sender, DevExpress.Xpf.PropertyGrid.CellValueChangedEventArgs args)
        {
            var items = treeListControl.SelectedItems;

            foreach (var item in items)
            {
                var uu = typeof (SysObjectConfig).GetProperty(args.Row.Path);
                if (uu == null)
                    continue;

                uu.SetValue(item, args.NewValue, null);
                var obj = item as SysObjectConfig;
                if (obj != null && !obj.IsDirty)
                    obj.IsDirty = true;
            }
            treeListControl.RefreshData();
            var vm = DataContext as IPropertyViewModel;
            if (vm != null)
                vm.RaiseCommandsCanExecuteChanged();
        }
    }
}
