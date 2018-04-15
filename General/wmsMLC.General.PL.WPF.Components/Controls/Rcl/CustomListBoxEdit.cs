using System.Collections;
using DevExpress.Xpf.Editors;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomListBoxEdit : ListBoxEdit
    {
        public int RowsCount
        {
            get
            {
                var rows = ItemsSource as IList;
                return rows == null ? 0 : rows.Count;
            }
        }

        //protected override void OnLoadedInternal()
        //{
        //    base.OnLoadedInternal();
        //    if (ListBoxCore != null)
        //        ListBoxCore.AllowItemHighlighting = false;
        //}

        //protected override object CoerceEditValue(System.Windows.DependencyObject d, object value)
        //{
        //    var result = base.CoerceEditValue(d, value);
        //    return result;
        //}
     
        /// <summary>
        /// Устанавливаем фокус на текущую строку.
        /// </summary>
        public void SetSelected(int index)
        {
            SelectedIndex = index;
            if ( ListBoxCore == null ||  ListBoxCore.ItemContainerGenerator == null || SelectedIndex < 0)
                return;

           UpdateSelectedItemFocus();
        }

        public new void Focus()
        {
            if (ListBoxCore != null)
                ListBoxCore.Focus();
        }

        public ListBoxEditItem GetItemByIndex(int index)
        {
            if (ListBoxCore == null || ListBoxCore.ItemContainerGenerator == null || index < 0)
                return null;
            return ListBoxCore.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxEditItem;
        }

        private void UpdateSelectedItemFocus()
        {
            var listBoxItem = GetItemByIndex(SelectedIndex);
            if (listBoxItem != null)
                listBoxItem.Focus();
        }
    }
}
