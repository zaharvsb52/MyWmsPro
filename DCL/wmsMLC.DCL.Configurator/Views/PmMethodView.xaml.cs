using System;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using wmsMLC.DCL.Configurator.ViewModels;

namespace wmsMLC.DCL.Configurator.Views
{
    public partial class PmMethodView
    {

        public PmMethodView()
        {
            InitializeComponent();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var listbox = sender as ListBoxEdit;
            if (listbox == null)
                return;

            if (listbox.SelectedIndex  < 0)
                return;

            switch (e.Key)
            {
                case Key.Space:
                    var data = listbox.SelectedItem as PmMethodViewModel.CheckedPmMethod;
                    if (data == null)
                        return;
                    data.IsChecked = !data.IsChecked;
                    e.Handled = true;
                    break;
                case Key.Up:
                    var index = listbox.SelectedIndex - 1;
                    if (index >= 0)
                        listbox.SelectedIndex = index;
                    e.Handled = true;
                    break;
                case Key.Down:
                    var items = listbox.ItemsSource as IList;
                    int count;
                    if (items != null && (count =items.Count) > 0)
                    {
                        index = listbox.SelectedIndex + 1;
                        if (index < count)
                            listbox.SelectedIndex = index;
                    }
                    e.Handled = true;
                    break;
            }
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listbox = sender as ListBoxEdit;
            if (listbox == null)
                return;

            var uie = listbox.InputHitTest(e.GetPosition(listbox)) as FrameworkElement;
            if (uie == null)
                return;
            var item = LayoutHelper.FindParentObject<ListBoxEditItem>(uie);
            if (item == null)
                return;

            //if (!item.IsSelected)
            //    item.IsSelected = true;
            //if (!item.IsFocused)
            //    item.Focus();

            var data = item.DataContext as PmMethodViewModel.CheckedPmMethod;
            if (data == null)
                return;
            data.IsChecked = !data.IsChecked;
            e.Handled = true;
        }

        private void OnItemsSourceChanged(object sender, EventArgs e)
        {
            var listbox = sender as ListBoxEdit;
            if (listbox == null || listbox.ItemsSource == null)
                return;
            listbox.SelectedIndex = 0;
        }
    }
}
