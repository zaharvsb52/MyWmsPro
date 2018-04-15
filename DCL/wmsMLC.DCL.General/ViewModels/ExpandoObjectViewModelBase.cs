using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.DCL.General.ViewModels
{
    public class ExpandoObjectViewModelBase : SourceViewModelBase<CustomExpandoObject>, ICustomModelHandler, ISettingsNameHandler, IObjectViewModel, IPropertyEditHandler
    {
        #region .  Fields  .
        private ObservableCollection<ValueDataField> _fields;
        private List<ValueDataField> _menuItems;
        private string _suffix;
        #endregion . Fields .

        #region .  Proeprties  .

        public IPropertyChangeHandler PropertyChangeHandler { get; set; }
        public object this[string key]
        {
            get
            {
                var result = Source.Members;
                if (!result.ContainsKey(key))
                    return null;

                return result[key];
            }
            set
            {
                var source = Source.Members;
                if (source.ContainsKey(key))
                    source[key] = value;
                else
                    source.Add(key, value);
                var field = Fields.FirstOrDefault(i => i.Name.EqIgnoreCase(key));
                if (field != null && field.Value != value)
                    field.Value = value;
                Source.OnSourcePropertyChanged(key);
            }
        }

        public object ParentViewModelSource { get; set; }

        public SettingDisplay DisplaySetting { get; set; }

        public string LayoutValue { get; set; }

        public bool InsertFromAvailableItems { get; set; }

        public ObservableCollection<ValueDataField> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                foreach (var field in _fields)
                    this[field.Name] = field.Value;
            }
        }

        public List<ValueDataField> MenuItems
        {
            get { return _menuItems ?? (_menuItems = new List<ValueDataField>()); }
        }

        public string MenuResult { get; private set; }

        public virtual Action<string> MenuAction { get; set; }

        public ICommand MenuCommand { get; set; }

        #endregion .  Proeprties  .


        public ExpandoObjectViewModelBase()
        {
            Source = new CustomExpandoObject();
            Source.PropertyChanged += Source_PropertyChanged;
            DisplaySetting = SettingDisplay.Detail;

            MenuCommand = new DelegateCustomCommand<object>(OnMenuCommand, CanMenuCommand);
        }

        #region . Methods .

        void Source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                WaitStart();
                var field = Fields.FirstOrDefault(i => i.Name.EqIgnoreCase(e.PropertyName));
                if (field != null)
                {
                    if (PropertyChangeHandler != null && field.IsOnPropertyChange)
                        PropertyChangeHandler.OnPropertyChange(this, e.PropertyName);
                }
            }
            finally
            {
                WaitStop();
            }
        }

        protected override void InitializeSettings()
        {
            base.InitializeSettings();

            // по умолчанию отключаем настройку. Должно настраиваться извне
            IsCustomizeBarEnabled = false;
        }

        public virtual T Get<T>(string key)
        {
            return (T)SerializationHelper.ConvertToTrueType(this[key], typeof(T));
        }

        protected override MenuViewModel CreateMenuViewModel()
        {
            return new MenuViewModel(MenuSuffix) {NotUseGlobalLayoutSettings = true};
        }

        /// <summary>
        /// Создаем настроенное из вне меню
        /// </summary>
        public void CreateCustomMenu()
        {
            // сбросим значение результата меню
            MenuResult = null;
            if (MenuItems == null || MenuItems.Count == 0)
                return;

            // получаем или создаем панель для настраиваемых кнопок
            var bar = Menu.GetOrCreateBarItem("Custom", 0, "CustomBar");

            // добавляем на панель новые кнопки
            foreach (var item in MenuItems)
            {
                // если кнопка уже есть - пропускаем
                var changingItem = bar.MenuItems.FirstOrDefault(i => i.Name == item.Name);
                if (changingItem != null)
                {
                    changingItem.IsEnable = !item.IsEnabled.HasValue || item.IsEnabled.Value;
                    changingItem.IsVisible = item.Visible;
                    continue;
                }

                var hotkey = item.Value.To(Key.None);
                bar.MenuItems.Add(new CommandMenuItem
                {
                    Name = item.Name,
                    Caption = item.Caption,
                    HotKey = hotkey == Key.None ? null : new KeyGesture(hotkey),
                    Command = MenuCommand,
                    CommandParameter = hotkey == Key.None ? null : hotkey.ToString(),
                    ImageSmall = string.IsNullOrEmpty(item.ImageName)
                             ? ImageResources.DCLAddNew16.GetBitmapImage()
                             : ResourceHelper.GetImageByName("wmsMLC.DCL.Resources", "ImageResources", string.Format("{0}16", item.ImageName)) ?? ImageResources.DCLDefault16.GetBitmapImage(),
                    ImageLarge = string.IsNullOrEmpty(item.ImageName)
                             ? ImageResources.DCLAddNew32.GetBitmapImage()
                             : ResourceHelper.GetImageByName("wmsMLC.DCL.Resources", "ImageResources", string.Format("{0}32", item.ImageName)) ?? ImageResources.DCLDefault16.GetBitmapImage(),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    IsEnable = !item.IsEnabled.HasValue || item.IsEnabled.Value,
                    IsVisible = item.Visible
                });
            }

            // сообщаем, что меню поменялись
            OnPropertyChanged(MenuPropertyName);
        }

        private bool CanMenuCommand(object parameter)
        {
            return !WaitIndicatorVisible && Fields != null && Fields.Count > 0;
        }

        private void OnMenuCommand(object parameter)
        {
            if (parameter.GetType().IsInstanceOfType(new KeyValuePair<string, object>()))
            {
                var keyValue = (KeyValuePair<string, object>)parameter;

                var value = keyValue.Value.To<string>();
                MenuResult = null;
                if (!CanMenuCommand(parameter))
                    return;

                MenuResult = value;
                var field = Fields.FirstOrDefault(i => i.Name == keyValue.Key);
                var isOnPropertyChange = false;
                if (field != null)
                    isOnPropertyChange = field.IsOnPropertyChange;
                if (MenuAction != null && !isOnPropertyChange)
                    MenuAction(value);
                else
                {
                    if (field != null)
                        this[field.Name] = Guid.NewGuid();
                }
            }
            else
            {
                MenuResult = null;
                if (!CanMenuCommand(parameter))
                    return;

                MenuResult = (string)parameter;
                if (MenuAction != null)
                    MenuAction((string)parameter);
            }


        }

        public void SetSuffix(string suffix)
        {
            MenuSuffix =
                _suffix = suffix;
        }

        public string GetSuffix()
        {
            return _suffix ?? string.Empty;
        }

        public ValueDataField GetFieldByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            //Single используется сознательно
            return Fields.Single(p => name.EqIgnoreCase(p.Name));
        }

        protected virtual bool CheckOnCriticalErrors(object obj)
        {
            if (Source.Validator == null)
                return true;

            Source.Validate();

            // если критических ошибок нет - можно продолжать
            if (!Source.Validator.HasCriticalError())
            {
                return true;
            }

            var desc = new StringBuilder();
            foreach (var validateError in Source.Validator.Errors)
            {
                if (!validateError.Value.HasCriticalErrors)
                    continue;

                // получаем описание
                desc.AppendLine(GetFieldByName(validateError.Key).Caption + ":");
                // получаем ошибки
                foreach (var v in validateError.Value)
                    desc.AppendLine("  - " + v.Description);
            }

            GetViewService().ShowDialog
                (StringResources.Error
                    , string.Format("{0}{1}{2}", StringResources.ErrorSave, Environment.NewLine, desc)
                    , MessageBoxButton.OK
                    , MessageBoxImage.Error
                    , MessageBoxResult.Yes);

            return false;
        }
        #endregion . Methods .

        #region . IObjectViewModel .

        public ObjectViewModelMode Mode { get; set; }

        public virtual bool DoAction()
        {
            return CheckOnCriticalErrors(this);
        }

        public ICommand DoActionCommand { get; private set; }
        public ObservableCollection<DataField> GetDataFields(SettingDisplay displaySetting)
        {
            return null;
        }

        public bool IsAdd { get; set; }
        public WMSBusinessObject SourceBase { get; set; }
        public bool IsNeedRefresh { get; set; }
        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;
        public event EventHandler NeedRefresh;

        public void InitializeMenus()
        {
            // INFO: InitializeCustomizationBar выставляеися при IsCustomizeBarEnabled
            InitializeContextMenu();
        }
        #endregion

        #region .  IPropertyEditHandler  .

        public bool InPropertyEditMode { get; set; }

        protected IEnumerable<WMSBusinessObject> PropertyEditSource { get; set; }
        public void SetSource(IEnumerable source)
        {
            InPropertyEditMode = true;
            PropertyEditSource = source.Cast<WMSBusinessObject>();
        }

        public virtual bool IsMergedPropery(string propertyName)
        {
            if (!InPropertyEditMode && PropertyEditSource == null)
                return false;
            return PropertyEditSource.Select(i => i.GetProperty(propertyName)).Distinct().ToArray().Length == 1;
        }
        #endregion .  IPropertyEditHandler  .
    }

    public class ExpandoObjectValidateViewModel : ExpandoObjectViewModelBase, IObjectViewModel
    {
        #region .  Ctors  .
        public ExpandoObjectValidateViewModel() { }

        public ExpandoObjectValidateViewModel(ExpandoObjectViewModelBase model)
            : this()
        {
            if (model != null)
                Fields = model.Fields;
        }
        #endregion .  Ctors  .

        #region .  Proeprties  .
        ICommand IActionHandler.DoActionCommand { get { return null; } }
        public bool GetIsMergedProperyFromField { get; set; }
        #endregion .  Proeprties  .

        #region .  Methods  .
        public override T Get<T>(string key)
        {
            var dict = Source.Members;
            if (dict.ContainsKey(key))
            {
                var value = dict[key];
                return value != null ? (T)value : default(T);
            }
            return default(T);
        }

        public override bool IsMergedPropery(string propertyName)
        {
            if (InPropertyEditMode && Fields != null && GetIsMergedProperyFromField)
            {
                var field = GetFieldByName(propertyName);
                if (field == null)
                    return false;
                return field.Get<bool>(ValueDataFieldConstants.IsMergedProperty);
            }

            return base.IsMergedPropery(propertyName);
        }

        public T Get<T>(string key, T oldvalue)
        {
            var dict = Source.Members;
            if (dict.ContainsKey(key))
            {
                var value = dict[key];
                //return value != null ? (T)value : default(T);
                return value != null ? (T)value : oldvalue;
            }
            return oldvalue;
        }

        public override bool DoAction()
        {
            var errors = new List<string>();
            foreach (var field in Fields.Where(p => p.IsRequired))
            {
                var message = ValidateRequiredField(field);
                if (!string.IsNullOrEmpty(message))
                    errors.Add(message);
            }

            if (errors.Count == 0)
            {
                foreach (var field in Fields.Where(p => p.DependentFields != null && p.DependentFields.Length > 0))
                {
                    if (Equals(field.Value, this[field.Name]))
                        continue;
                    foreach (var fn in field.DependentFields.Where(f => !string.IsNullOrEmpty(f)))
                    {
                        var f = Fields.SingleOrDefault(p => p.Name.EqIgnoreCase(fn));
                        if (f != null)
                        {
                            var message = ValidateRequiredField(f);
                            if (!string.IsNullOrEmpty(message))
                                errors.Add(message);
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                var message = string.Join(Environment.NewLine, errors.Distinct());
                GetViewService().ShowDialog(StringResources.Error
                    , string.Format("{0}{1}{2}", StringResources.ErrorSave, Environment.NewLine, message)
                    , MessageBoxButton.OK
                    , MessageBoxImage.Error
                    , MessageBoxResult.Yes);
                return false;
            }

            foreach (var f in Fields)
            {
                f.Value = this[f.Name];
            }
            return true;
        }

        private string ValidateRequiredField(ValueDataField field)
        {
            var haserror = false;
            var value = this[field.Name];
            if (Equals(value, string.Empty))
                value = null;

            if (field.FieldType == typeof(string))
            {
                if (value.To<string>().IsNullOrEmptyAfterTrim())
                    haserror = true;
            }

            if (haserror || value == null)
                return string.Format("Поле '{0}' является обязательным.", field.Caption);
            return string.Empty;
        }
        #endregion . Methods .
    }
}
