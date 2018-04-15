using System;
using System.Collections.Specialized;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;

namespace wmsMLC.Activities.Business.Views.Editors
{
    public partial class EntityTypeCollectionEditorDialog
    {
        private readonly ObservableCollection<ValueDataField> _collection;

        public EntityTypeCollectionEditorDialog(ObservableCollection<ValueDataField> collection)
        {
            InitializeComponent();
            _collection = collection;
            GetEntities();
            _dataGrid.DataContext = collection;
        }

        public ObservableCollection<ValueDataField> Collection
        {
            get { return _collection; }
        }

        private void GetEntities()
        {
            var objectManager = IoC.Instance.Resolve<IManagerForObject>();
            using (var mgr = IoC.Instance.Resolve<IBaseManager<SysObject>>())
            {
                var type = typeof(SysObject);
                var data = mgr.GetFiltered(string.Format("{0} = 0 AND {1} != '{2}'", SourceNameHelper.Instance.GetPropertySourceName(type, SysObject.ObjectDataTypePropertyName),
                    SourceNameHelper.Instance.GetPropertySourceName(type, SysObject.ObjectNamePropertyName), type.Name.ToUpper()), GetModeEnum.Partial);
                _entityTypeCol.ItemsSource = data.Select(p => new
                    {
                        Name = p.ObjectName,
                        Description = GetEntityDescription(objectManager, p.ObjectName)
                    }).OrderBy(p => p.Description).ToList();
                _actionCol.ItemsSource = Enum.GetValues(typeof (RefreshAction)).Cast<RefreshAction>().ToArray();
            }
        }

        private string GetEntityDescription(IManagerForObject objectManager, string entity)
        {
            var type = objectManager.GetTypeByName(entity);
            return type == null ? entity : string.Format("{0} ({1})", type.GetDisplayName(), entity);
        }

        private void _buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            _collection.Add(new ValueDataField {IsEnabled = true, LabelPosition = "Left"});
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
