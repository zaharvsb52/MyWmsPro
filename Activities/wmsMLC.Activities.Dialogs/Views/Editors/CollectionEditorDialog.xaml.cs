using System;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Dialogs.Views.Editors
{
    public partial class CollectionEditorDialog
    {
        private readonly ObservableCollection<ValueDataField> _collection;

        public CollectionEditorDialog(ObservableCollection<ValueDataField> collection)
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
            _collection.Add(new ValueDataField {FieldType = typeof (string), IsEnabled = true, LabelPosition = "Left"});
        }

        private void _buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = _dataGrid.SelectedItem as ValueDataField;
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
            var selectedItem = _dataGrid.SelectedItem as ValueDataField;
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
            var selectedItem = _dataGrid.SelectedItem as ValueDataField;
            if (selectedItem == null)
                return;

            var index = _collection.IndexOf(selectedItem);
            if (index + 1 < _collection.Count)
            {
                _collection.Move(index, index + 1);
                _dataGrid.SelectedItem = selectedItem;
            }
        }

        private ValueDataFieldPropertiesEditorDialog.ValueDataFieldProperty CreateValueDataFieldProperty(KeyValuePair<string, object> property)
        {
            var result = new ValueDataFieldPropertiesEditorDialog.ValueDataFieldProperty
            {
                Name = property.Key,
                Type = property.Value == null ? typeof (object) : property.Value.GetType(),
                Value = property.Value,
                UseWfVariable = ValueDataFieldConstants.ValidateUseWfVariable(property.Value)
            };

            if (result.UseWfVariable)
            {
                result.Value = ValueDataFieldConstants.GetWfVariablePropertyName(result.Value);
                if (result.Type == typeof (string))
                    result.Type = typeof (object);
            }
            return result;
        }

        private void OnEditProperties(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var data = _dataGrid.SelectedItem as ValueDataField;
                if (data == null)
                    return;

                var clonedCollection = new ObservableCollection<ValueDataFieldPropertiesEditorDialog.ValueDataFieldProperty>(
                    data.Properties.Select(CreateValueDataFieldProperty));

                var dialog = new ValueDataFieldPropertiesEditorDialog(clonedCollection);
                if (dialog.ShowDialog() == true)
                {
                    data.Properties.Clear();
                    foreach (var item in clonedCollection.Where(p => !string.IsNullOrEmpty(p.Name)))
                    {
                        var value = item.UseWfVariable
                            ? string.Format("{0}{1}", ValueDataFieldConstants.WfVariableFlag, item.Value)
                            : SerializationHelper.ConvertToTrueType(item.Value, item.Type);

                        data.Set(item.Name, value);
                    }
                    button.DataContext = data;
                }
            }
        }
    }
}
