using System;
using System.Collections.ObjectModel;
using System.Windows;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    public partial class MenuCollectionEditorDialog
    {
        private readonly ObservableCollection<Field> _collection;

        public MenuCollectionEditorDialog(ObservableCollection<ValueDataField> collection)
        {
            _collection = new ObservableCollection<Field>();

            InitializeComponent();
            
            if (collection != null)
            {
                foreach (var p in collection)
                {
                    _collection.Add(new Field {Data = p});
                }
            }
            _dataGrid.DataContext = _collection;
        }

        public ObservableCollection<ValueDataField> Collection
        {
            get
            {
                var result = new ObservableCollection<ValueDataField>();
                foreach (var p in _collection)
                {
                    result.Add(p.Data);
                }
                return result;
            }
        }

        private void _buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            _collection.Add(new Field {Data = new ValueDataField {IsEnabled = true}});
        }

        private void _buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _dataGrid.SelectedItem as Field;
            if (selectedItem == null)
                return;

            if (MessageBox.Show
                    (
                        String.Format("Are you sure you want to delete selected item '{0}'?", selectedItem.Data.Name),
                        "Delete",
                        MessageBoxButton.OKCancel
                    ) == MessageBoxResult.OK)
            {
                _collection.Remove(selectedItem);
            }
        }

        private void _buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void _buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void _buttonUp_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _dataGrid.SelectedItem as Field;
            if (selectedItem == null)
                return;

            var index = _collection.IndexOf(selectedItem);
            if (index > 0)
            {
                _collection.Move(index, index - 1);
                _dataGrid.SelectedItem = selectedItem;
            }
        }

        private void _buttonDown_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _dataGrid.SelectedItem as Field;
            if (selectedItem == null)
                return;

            var index = _collection.IndexOf(selectedItem);
            if (index + 1 < _collection.Count)
            {
                _collection.Move(index, index + 1);
                _dataGrid.SelectedItem = selectedItem;
            }
        }

        public class Field
        {

            public ValueDataField Data { get; set; }

            public int Row
            {
                get
                {
                    return Data == null ? 0 : Data.Get<int>(ValueDataFieldConstants.Row);
                }
                set
                {
                    if (Data == null)
                        return;
                    Data.Set(ValueDataFieldConstants.Row, value);
                }
            }

            public int Column
            {
                get
                {
                    return Data == null ? 0 : Data.Get<int>(ValueDataFieldConstants.Column);
                }
                set
                {
                    if (Data == null)
                        return;
                    Data.Set(ValueDataFieldConstants.Column, value);
                }
            }
        }
    }
}
