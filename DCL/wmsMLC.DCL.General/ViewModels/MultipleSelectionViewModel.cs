using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.DCL.General.ViewModels
{
    public class MultipleSelectionViewModel<TModel> : SourceViewModelBase<TModel>
    {
        public class SelectionItem : ViewModelBase
        {
            public const string CheckedPropertyName = "Checked";
            public const string ItemObjectPropertyName = "ItemObject";
            public const string IsEnabledPropertyName = "IsEnabled";

            private bool _checked;
            private TModel _itemObject;
            private bool _isEnabled;

            public bool Checked
            {
                get { return _checked; }
                set
                {
                    _checked = value;
                    OnPropertyChanged(CheckedPropertyName);
                }
            }

            public TModel ItemObject
            {
                get { return _itemObject; }
                set
                {
                    _itemObject = value;
                    OnPropertyChanged(ItemObjectPropertyName);
                }
            }

            public bool IsEnabled
            {
                get { return _isEnabled; }
                set
                {
                    _isEnabled = value;
                    OnPropertyChanged(IsEnabledPropertyName);
                }
            }

            public SelectionItem(TModel item)
            {
                ItemObject = item;
            }
        }

        public MultipleSelectionViewModel()
        {
            AllowClosePanel = true;
            PanelCaption = StringResources.MultipleSelectionViewModelPanelCaption;
            PanelCaptionImage = ImageResources.DCLDefault16.GetBitmapImage();
            Items = new ObservableCollection<SelectionItem>();
            Fields = new ObservableCollection<DataField>();
            var checkField = new DataField
                {
                    Caption = StringResources.Select,
                    FieldType = typeof(bool?),
                    Name = SelectionItem.CheckedPropertyName,
                    FieldName = SelectionItem.CheckedPropertyName,
                    IsEnabled = true,
                    Visible = true,
                };
            Fields.Add(checkField);

            AddMenu();
        }

        public void AddFields(IEnumerable<DataField> fields)
        {
            foreach (var field in fields)
            {
                // HACK: необходимо отделяться от кешируемой коллекции полей, потому что изменяем имя поля
                var df = new DataField
                {
                    Caption = field.Caption,
                    Description = field.Description,
                    DisplayFormat = field.DisplayFormat,
                    FieldType = field.FieldType,
                    IsEnabled = false,
                    SourceName = field.SourceName,
                    FieldName = string.Format("{0}.{1}", SelectionItem.ItemObjectPropertyName, field.Name),
                    Name = field.SourceName,
                    Visible = true,
                };
                Fields.Add(df);
            }
        }

        public void AddField(string name, bool isEnabled = false)
        {
            var hackField = new DataField { Caption = name, Name = name, IsEnabled = isEnabled };
            //INFO: склеивал имена для биндинга с вложением
            //hackField.Name = string.Format("{0}.{1}", SelectionItem.ItemObjectPropertyName, name.ToUpper());
            Fields.Add(hackField);
        }

        public ObservableCollection<SelectionItem> Items { get; set; }
        public ObservableCollection<DataField> Fields { get; private set; }

        public void SetSource(string key, IEnumerable<TModel> items, IEnumerable<TModel> excludeItems)
        {
            var properties = TypeDescriptor.GetProperties(typeof(TModel));
            var property = properties.Cast<PropertyDescriptor>().FirstOrDefault(i => i.Name.EqIgnoreCase(key));
            if (property != null)
            {
                foreach (var item in items)
                {
                    var itemvalue = property.GetValue(item);
                    var selectionItem = new SelectionItem(item)
                    {
                        IsEnabled = (excludeItems != null) ? !excludeItems.Any(i => Equals(itemvalue, property.GetValue(i))) : true
                    };
                    selectionItem.Checked = !selectionItem.IsEnabled;
                    Items.Add(selectionItem);
                }
            }
        }

        private void AddMenu()
        {
            InitializeCustomizationBar();
        }

        protected override void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu)
        {
            base.SetupCustomizeMenu(bar, listmenu);
            
            // убираем функционал настройки самого layout-а
            if (listmenu != null && listmenu.MenuItems != null)
            {
                var item = listmenu.MenuItems.FirstOrDefault(i => i.Caption == StringResources.CustomizeRegion);
                if (item != null)
                    listmenu.MenuItems.Remove(item);
            }
        }
    }
}
