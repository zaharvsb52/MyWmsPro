using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.General.PL.WPF.Components.ViewModels
{
    public class RclListViewModelBase<T> : RclPanelViewModelBase, IRclListViewModel where T : class
    {
        public RclListViewModelBase()
        {
            ItemSelectCommand = new DelegateCustomCommand(this, OnItemSelect, CanItemSelect);
        }

        #region .  Properties  .
        public Type ItemsSourceType
        {
            get { return typeof(T); }
        }

        private List<T> _itemsSource;
        public List<T> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource == value)
                    return;
                _itemsSource = value;
                OnPropertyChanged("ItemsSource");
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public virtual Action<ValueDataField> MenuAction { get; set; }
        public ICommand ItemSelectCommand { get; private set; }
        #endregion .  Properties  .

        private bool CanItemSelect()
        {
            return !WaitIndicatorVisible && SelectedItem != null;
        }

        private void OnItemSelect()
        {
            if (!CanItemSelect())
                return;

            if (MenuAction != null)
                MenuAction(ValueDataFieldConstants.CreateDefaultParameter(string.Empty));
        }

        public override void SetItemsSource(object source)
        {
            base.SetItemsSource(source);
            if (source == null)
            {
                ItemsSource = null;
                return;
            }

            var data = source as IEnumerable;
            if (data == null) 
                return;
            var list = data.OfType<T>().ToArray();
            if (list.Any())
                ItemsSource = new List<T>(list);
        }
    }
}
