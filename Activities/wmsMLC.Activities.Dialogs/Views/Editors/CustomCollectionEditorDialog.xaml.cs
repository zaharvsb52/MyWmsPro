using System;
using System.Activities.Presentation;
using System.Collections.ObjectModel;
using System.Windows;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    /// <summary>
    /// Interaction logic for CustomCollectionEditorDialog.xaml
    /// </summary>
    public partial class CustomCollectionEditorDialog : Window
    {
        private readonly ObservableCollection<ValueDataField> _collection;

        public CustomCollectionEditorDialog(ObservableCollection<ValueDataField> collection)
        {
            InitializeComponent();

            LayoutRoot.DataContext = new EditingContext();

            _collection = collection;
            _dataGrid.DataContext = collection;
        }

        public ObservableCollection<ValueDataField> Collection
        {
            get { return _collection; }
        }

        private void _buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            _collection.Add(new ValueDataField { IsEnabled = true, LabelPosition = "Left" });
        }

        private void _buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_dataGrid.SelectedItem is ValueDataField)
            {
                var selectedItem = _dataGrid.SelectedItem as ValueDataField;

                if (MessageBox.Show
                        (
                            String.Format("Are you sure you want to delete selected item '{0}'?", selectedItem.Name),
                            "Delete",
                            MessageBoxButton.OKCancel
                        ) == MessageBoxResult.OK)
                {
                    _collection.Remove(selectedItem);
                }
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
            if (_dataGrid.SelectedItem is ValueDataField
             && (_collection.IndexOf(_dataGrid.SelectedItem as ValueDataField) > 0))
            {
                var item = _dataGrid.SelectedItem as ValueDataField;

                var index = _collection.IndexOf(item);
                _collection.Move(index, index - 1);
                _dataGrid.SelectedItem = item;
            }
        }

        private void _buttonDown_Click(object sender, RoutedEventArgs e)
        {
            if (_dataGrid.SelectedItem is ValueDataField
             && (_collection.IndexOf(_dataGrid.SelectedItem as ValueDataField) + 1 < _collection.Count))
            {
                var item = _dataGrid.SelectedItem as ValueDataField;

                var index = _collection.IndexOf(item);
                _collection.Move(index, index + 1);
                _dataGrid.SelectedItem = item;
            }
        }
    }
}
