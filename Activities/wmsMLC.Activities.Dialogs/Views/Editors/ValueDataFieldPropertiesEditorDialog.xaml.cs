using System;
using System.Activities.Presentation;
using System.Collections.ObjectModel;
using System.Windows;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    public partial class ValueDataFieldPropertiesEditorDialog
    {
        private readonly ObservableCollection<ValueDataFieldProperty> _collection;

        public ValueDataFieldPropertiesEditorDialog(ObservableCollection<ValueDataFieldProperty> collection)
        {
            InitializeComponent();

            //var ec = new EditingContext();
            //var mtm = new ModelTreeManager(ec); 
            //ModelItem mi = mtm.Root;
            
            LayoutRoot.DataContext = new EditingContext();

            _collection = collection;
            _dataGrid.DataContext = collection;
        }

        public ObservableCollection<ValueDataFieldProperty> Collection
        {
            get { return _collection; }
        }

        private void _buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            _collection.Add(new ValueDataFieldProperty {Type = typeof (string)});
        }

        private void _buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _dataGrid.SelectedItem as ValueDataFieldProperty;
            if (selectedItem == null)
                return;

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
            var selectedItem = _dataGrid.SelectedItem as ValueDataFieldProperty;
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
            var selectedItem = _dataGrid.SelectedItem as ValueDataFieldProperty;
            if (selectedItem == null)
                return;

            var index = _collection.IndexOf(selectedItem);
            if (index + 1 < _collection.Count)
            {
                _collection.Move(index, index + 1);
                _dataGrid.SelectedItem = selectedItem;
            }
        }

        public class ValueDataFieldProperty
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public object Value { get; set; }
            public bool UseWfVariable { get; set; }
        }
    }
}
