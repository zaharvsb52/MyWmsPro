using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.General;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.Properties;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views
{
    public class CustomSubListControl : CustomSubControl, INotifyPropertyChanged, ISettingsNameHandler
    {
        #region .  Consts  .

        public const string ParentViewModelSourcePropertyName = "ParentViewModelSource";

        #endregion

        #region .  Fields  .

        private bool _isReadOnly;
        private EditableBusinessObject _parentViewModelSource;
        private ObservableCollection<DataField> _fields;
        private object _currentItem;
        private bool _shouldUpdateSeparately;

        protected Type _itemsType;
        protected Type _itemType;

        #endregion .  Fields  .

        #region .  Properties  .

        public IList SelectedItems { get; protected set; }
        public string SubListParentFieldName { get; set; }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly == value)
                    return;
                _isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        public ObservableCollection<DataField> Fields
        {
            get { return _fields; }
            set
            {
                if (value == _fields)
                    return;
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        public object CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem == value)
                    return;
                _currentItem = value;
                OnPropertyChanged("CurrentItem");

                var selectableSublistView = ParentViewModel as ISelectableSublistView;
                if (selectableSublistView != null)
                {
                    selectableSublistView.SelectedItems[SubListParentFieldName] = (object)value;
                    if (selectableSublistView.OnChangeSelectItem != null)
                    {
                        selectableSublistView.OnChangeSelectItem(SubListParentFieldName, new EventArgs());
                    }
                }
            }
        }

        public MenuViewModel Menu { get; set; }

        public IModelHandler ParentViewModel { get; set; }

        /// <summary>
        /// ћожно ли этот объект сохран€ть отдельно от основного.
        /// </summary>
        public bool ShouldUpdateSeparately
        {
            get { return _shouldUpdateSeparately; }
            set
            {
                _shouldUpdateSeparately = value;
                // INFO: дадим возвожность править в таблице всегда
                // пусть побудет десь до апрува
                //_gridEditMenuItem.IsVisible = _shouldUpdateSeparately;
                OnPropertyChanged("ShouldUpdateSeparately");
            }
        }

        public EditableBusinessObject ParentViewModelSource
        {
            get { return _parentViewModelSource; }
            set
            {
                _parentViewModelSource = value;
                OnPropertyChanged(ParentViewModelSourcePropertyName);
            }
        }

        /// <summary>
        /// ѕереопределение модели редактировани€.
        /// </summary>
        public Func<IObjectViewModel> CustomEditModel { get; set; }

        #endregion . Properties .

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .
        
        #region .  Methods  .

        public static IViewService GetViewService()
        {
            return IoC.Instance.Resolve<IViewService>();
        }

        public void SubscribeSource()
        {
            if (ParentViewModelSource != null)
                ParentViewModelSource.PropertyChanged += OnPropertyChanged;
        }

        public void UnSubscribeSource()
        {
            if (ParentViewModelSource != null)
                ParentViewModelSource.PropertyChanged -= OnPropertyChanged;
        }
       
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            var commands = GetType().GetProperties().Where(p => p.PropertyType == typeof(ICommand));
            foreach (var p in commands)
            {
                RaiseCanExecuteChanged((ICommand)p.GetGetMethod().Invoke(this, null));
            }
        }

        public void RaiseCanExecuteChanged(ICommand command)
        {
            var dc = command as ICustomCommand;
            if (dc == null)
                return;
            dc.RaiseCanExecuteChanged();
        }
        

        public void SetValueByLookup(object item, PropertyDescriptor property, object value)
        {
            if (property == null)
                return;
            property.SetValue(item, value);
        }

        public PropertyDescriptor[] IsLookupExist(object item, string parententityname)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (string.IsNullOrEmpty(parententityname))
                throw new ArgumentNullException("parententityname");

            var result = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(_itemType))
            {
                // определ€ем, что перед нами Lookup
                var lookupAttribute = property.Attributes[typeof(LookupAttribute)] as LookupAttribute;
                if (lookupAttribute != null)
                {
                    var lookUpCode = lookupAttribute.LookUp;
                    if (string.IsNullOrEmpty(lookUpCode))
                        continue;

                    // получаем описание Lookup-а
                    var lookupInfo = LookupHelper.GetLookupInfo(lookUpCode);
                    if (parententityname.EqIgnoreCase(lookupInfo.ItemSource))
                    {
                        result.Add(property);
                    }
                }
            }
            return result.ToArray();
        }

        public virtual void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ParentViewModelSource = DataContext as WMSBusinessObject;

            if (ParentViewModelSource != null)
            {
                //HACK: »спользуетс€ дл€ VarFilter и многого другого
                RaiseCanExecuteChanged();
                SubscribeSource();

                var members = (ParentViewModelSource.GetType()).GetMembers();
                var prop = members.FirstOrDefault(m => m.Name.ToUpper().Equals(SubListParentFieldName));
                if (prop != null)
                {
                    var attributes = prop.GetCustomAttributes(typeof(GCFieldAttribute), true);
                    if (attributes.Length == 1)
                    {
                        ShouldUpdateSeparately = false;
                    }
                }
            }
        }

        public string GetSuffix()
        {
            return _itemType == null ? null : _itemType.FullName;
        }

        public void OnNeedRefresh(object sender, EventArgs eventArgs)
        {
            if (ShouldUpdateSeparately)
            {
                ParentRefresh();
            }
            else
                Revalidate();
        }

        public void ParentRefresh()
        {
            if (ParentViewModel != null)
                ParentViewModel.RefreshData();
        }

        public void Revalidate()
        {
            if (ParentViewModel == null)
                return;

            //INFO: заставим родител€ выполнить валидацию
                var validatable = ParentViewModel.GetSource() as IValidatable;
            if (validatable != null)
                validatable.Validate();
        }

        public IBaseManager GetManager(Type type)
        {
            var mto = IoC.Instance.Resolve<IManagerForObject>();
            var mgrType = mto.GetManagerByTypeName(type.Name);

            object tryRes;
            if (IoC.Instance.TryResolve(mgrType, out tryRes))
                return (IBaseManager) tryRes;
            return null;
        }

        public void SetItemType(Type itemType)
        {
            if(itemType == null)
                throw new ArgumentNullException("itemType");
            _itemType = itemType;
        }

        #endregion .  Methods  .
    }
}