using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.EditStrategy;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Editors.Validation.Native;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.LookUp;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Attributes;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

#pragma warning disable 4014

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomLookUpEdit : LookUpEdit, IDisposable, ISaveRestore
    {
        #region .  Fields & Dependecy properties  .
        public static readonly DependencyProperty LookUpCodeEditorProperty = DependencyProperty.Register("LookUpCodeEditor", typeof(string), typeof(CustomLookUpEdit), new PropertyMetadata(OnLookUpCodeEditorChanged));
        public static readonly DependencyProperty LookUpCodeEditorFilterExtProperty = DependencyProperty.Register("LookUpCodeEditorFilterExt", typeof(string), typeof(CustomLookUpEdit), new PropertyMetadata(OnLookUpCodeEditorFilterExtChanged));

        private static void OnLookUpCodeEditorFilterExtChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = ((CustomLookUpEdit) d);
            if (DesignerProperties.GetIsInDesignMode(control))
                return;

            control.SetFilter(control.LookUpCodeEditorFilterExt);
        }

        public static readonly DependencyProperty LookUpColumnsSourceProperty = DependencyProperty.Register("LookUpColumnsSource", typeof(ObservableCollection<DataField>), typeof(CustomLookUpEdit));

        private static readonly Brush WaitingBackgroundColor = Brushes.LightPink;
        private const string Button3PointName = "Button3Point";

        private IBaseManager _managerInstance;
        private string _filter0;
        private string _filterInternal;
        private bool _isPkg;
        private volatile Brush _previosBrush;
        private string _primaryKeyPropertyName;

        private readonly ICustomCommand _openReferenceWindowCommand;
        private bool _isSettingLoaded;
        private bool _previosIncrementalFiltering;
        private string _displayValueForFilteringAfterPopupOpened;

        private GridControl _popupContentGridControl;

        /// <summary>
        /// VarFilter. Формат: DefaultFilter [$PropertyName1,(Value11, Filter11),(Value12,Filter12),...],[$PropertyName2,(Value21, Filter21),(Value22,Filter22),...],...
        ///, где
        ///DefaultFilter - Основной фильтр (если есть)
        ///$PropertyName1 - имя свойства сущности,
        ///Value11 - значение данного свойства,
        ///Filter11 - фильтр для данного значения.
        /// Если используется функция, то в качестве разделителей параметров используется ';' (без кавычек).
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _varFilter;

        #endregion .  Fields  .

        public CustomLookUpEdit()
        {
            _isPkg = false;
            IsSimpleMode = true;
            AutoComplete = true;
            AllowSpinOnMouseWheel = false;
            ImmediatePopup = true;
            NullValue = null;
            PopupCloseMode = PopupCloseMode.Normal;
            AllowNullInput = true; // Gets or sets whether end-users can set the editor's value to a null reference by pressing the CTRL+DEL or CTRL+0 key combinations.
            MaxFetchItemsCount = 50;
            ClosePopupOnLostFocus = false;
            ValidateOnEnterKeyPressed = true;
            AutoComplete = false;

            // делаем, чтобы автопоиск осуществлялся по условию "содержит" (по-умолчанию "начинается с")
            FilterCondition = DevExpress.Data.Filtering.FilterCondition.Contains;

            DataContextChanged += OnDataContextChanged;
            EditValueChanged += OnCustomEditValueChanged;
            _openReferenceWindowCommand = new DelegateCustomCommand(OnShowWindow, CanShowWindow);

            PopupOpening += OnPopupOpening;
        }

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~CustomLookUpEdit()
        {
            if (_disposed)
                return;

            Dispose(false);
            _disposed = true;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">False - если требуется освободить только UnManaged ресурсы, True - если еще и Managed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataContextChanged -= OnDataContextChanged;
                EditValueChanged -= OnCustomEditValueChanged;
                UnSubscribeDataContext(DataContext);
                if (ManagerInstance != null)
                {
                    ManagerInstance.Changed -= OnChangedManagerInstance;
                    ManagerInstance.AllowMonitorChangesInOtherInsances = false;
                    ManagerInstance = null;
                }

                //Что б не очищался EditValue
                BindingOperations.ClearBinding(this, EditValueProperty);
                //ItemsSource = null;

                // прибиваем грид
                var dispGrid = PopupContentGridControl as IDisposable;
                if (dispGrid != null)
                    dispGrid.Dispose();
            }
            // unmanaged objects and resources
        }
        #endregion

        #region .  Properties  .
        protected Type ValueMemberType { get; set; }
        protected LookupInfo LookupInfo { get; set; }
        protected bool IsInWaiting { get; set; }

        /// <summary>
        /// Максимальное кол-во элементов, которое получается из БД.
        /// </summary>
        public int? MaxFetchItemsCount { get; set; }

        public object ParentViewModelSource { get; set; }

        public bool HasMoreData { get; private set; }

        protected bool InItemSourceChanging { get; set; }

        /// <summary>
        /// Код Lookup-а.
        /// </summary>
        public string LookUpCodeEditor
        {
            get { return (string)GetValue(LookUpCodeEditorProperty); }
            set { SetValue(LookUpCodeEditorProperty, value); }
        }

        /// <summary>
        /// Признак того, что Lookup-у не требуется получение данных (достаточно отображать только строку для текущего значения).
        /// </summary>
        public bool IsSimpleMode { get; protected set; }

        /// <summary>
        /// Имя виртуального поля, которое может заменить значение при отображении.
        /// </summary>
        public string VirtualFieldName { get; private set; }

        /// <summary>
        /// Добавочный безусловный фильтр.
        /// </summary>
        public string LookUpCodeEditorFilterExt
        {
            get { return (string)GetValue(LookUpCodeEditorFilterExtProperty); }
            set
            {
                SetValue(LookUpCodeEditorFilterExtProperty, value);
            }
        }

        public string LookUpCodeEditorVarFilterExt { get; set; }

        /// <summary>
        /// Источник данных для построения колонок (DependencyProperty).
        /// </summary>
        public ObservableCollection<DataField> LookUpColumnsSource
        {
            get { return (ObservableCollection<DataField>)GetValue(LookUpColumnsSourceProperty); }
            set { SetValue(LookUpColumnsSourceProperty, value); }
        }

        /// <summary>
        /// Отображать ли кнопку добавить.
        /// </summary>
        public bool LookupButtonEnabled { get; set; }

        /// <summary>
        /// Определение поиска по-умолчанию (Совместимость с версией 15.2).
        /// </summary>
        public bool UseNativeAutoCompleteSelection { get; set; } 

        protected string FilterInternal
        {
            get
            {
                return _filterInternal;
            }
            set
            {
                if (_filterInternal == value) 
                    return;
                _filterInternal = value;

                if (!IsSimpleMode)
                    RefreshData();
            }
        }

        private IBaseManager ManagerInstance
        {
            get { return _managerInstance; }
            set
            {
                if (_managerInstance != null)
                    _managerInstance.Changed -= OnChangedManagerInstance;

                _managerInstance = value;

                if (_managerInstance != null)
                    _managerInstance.Changed += OnChangedManagerInstance;
            }
        }

        /// <summary>
        /// Отображаемый в выпадающем окне грид. Если список не открывали - может быть null
        /// </summary>
        internal GridControl PopupContentGridControl
        {
            get
            {
                return _popupContentGridControl ?? (_popupContentGridControl = GetGridControl());
            }
        }

        public event RoutedEventHandler SourceChanged;

        protected new PopupCloseMode PopupCloseMode { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .

        private static void OnLookUpCodeEditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = ((CustomLookUpEdit)d);
            if (DesignerProperties.GetIsInDesignMode(control))
                return;

            control.InitializeLookUpCodeEditor();
        }

        public override void OnApplyTemplate()
        {
            ShowEditorButtons = true;
            base.OnApplyTemplate();

            if (LookupButtonEnabled)
            {
                var button3Point = ActualButtons == null ? null : ActualButtons.SingleOrDefault(p => Equals(p.Tag, Button3PointName));
                if (button3Point == null)
                {
                    button3Point = new ButtonInfo
                    {
                        GlyphKind = GlyphKind.Regular,
                        ButtonKind = ButtonKind.Simple,
                        Command = _openReferenceWindowCommand,
                        Tag = Button3PointName
                    };
                   
                    Buttons.Add(button3Point);
                }
            }

            OnEditButtonValidation();
        }

        protected override void OnShowEditButtonsChanged(bool oldValue)
        {
            //base.OnShowEditButtonsChanged(oldValue);
        }

        protected override void OnIsReadOnlyChanged()
        {
            base.OnIsReadOnlyChanged();
            OnEditButtonValidation();
        }

        protected override string GetDisplayText(object editValue, bool applyFormatting)
        {
            //Проверяем, если нажимали Ctrl+Del, то обрабатываем стандартным образом
            var editStrategy = EditStrategy as CustomLookUpEditStrategy;
            if (editStrategy != null && editStrategy.IsProcessNullInput)
                return base.GetDisplayText(editValue, applyFormatting);

            if (IsDisplayEqualValue())
                return editValue == null
                    ? null
                    : editValue is LookUpEditableItem
                        ? ((LookUpEditableItem)editValue).DisplayValue as string
                        : editValue.ToString();

            if (CanUseVirtualField())
            {
                object val = null;
                var bo = DataContext as BusinessObject;
                if (bo != null)
                    val = bo.GetProperty(VirtualFieldName);
                else
                {
                    var properties = TypeDescriptor.GetProperties(DataContext);
                    var property = properties[VirtualFieldName];
                    if (property != null)
                        val = property.GetValue(DataContext);
                }
                return val == null ? null : val.ToString();
            }
            
            var correctedEditValue = editValue is LookUpEditableItem ? editValue : GetCorrectedEditValue(EditValue);
            return base.GetDisplayText(correctedEditValue, applyFormatting);
        }

        private bool IsDisplayEqualValue()
        {
            return !string.IsNullOrEmpty(ValueMember) && DisplayMember == ValueMember;
        }

        protected override EditStrategyBase CreateEditStrategy()
        {
            return new CustomLookUpEditStrategy(this);
        }

        protected override void ItemsSourceChanged(object itemsSource)
        {
            base.ItemsSourceChanged(itemsSource);

            var h = SourceChanged;
            if (h != null)
                h(itemsSource, null);
        }

        protected override void SubscribeToSettings(BaseEditSettings settings)
        {
            base.SubscribeToSettings(settings);
            var setts = settings as CustomBaseLookupEditSetting;
            if (setts != null)
            {
                LookUpCodeEditor = setts.LookUpCodeEditor;
            }
        }

        private void OnPopupOpening(object sender, OpenPopupEventArgs e)
        {
            // предотвращаем открытие, если Lookup IsReadOnly
            var lookUpEdit = sender as LookUpEdit;
            if (lookUpEdit != null)
                e.Cancel = lookUpEdit.IsReadOnly;
        }

        protected override void OnPopupOpened()
        {
            base.OnPopupOpened();

            // загружаем вид
            if (!_isSettingLoaded)
            {
                SaveRestoreLayoutHelper.RestoreLayout(this, FormComponents.MenuAndComponents);
                _isSettingLoaded = true;
            }

            // если перед открытием нужно дозапросить данные, то делаем это
            if (!string.IsNullOrEmpty(_displayValueForFilteringAfterPopupOpened))
            {
                var filter = _displayValueForFilteringAfterPopupOpened;
                _displayValueForFilteringAfterPopupOpened = null;

                RefreshDataByDisplayMemverValue(filter);

                // восстанавиливаем необходимость автопоиска данных
                IncrementalFiltering = _previosIncrementalFiltering;
            }
            else
                if (IsSimpleMode)
                    RefreshData(false);

            if (PopupContentGridControl != null)
                PopupContentGridControl.KeyUp += PopupContentGridControlOnKey;
        }

        protected override void OnPopupClosed()
        {
            base.OnPopupClosed();

            if (PopupContentGridControl != null)
                PopupContentGridControl.KeyUp -= PopupContentGridControlOnKey;
        }

        private void PopupContentGridControlOnKey(object sender, KeyEventArgs ea)
        {
            // при нажатии на Enter в Grid-е обновляем данные
            // TODO: возможно стоит определять, что мы именно в строке поиска
            if (ea.Key == Key.Enter || ea.Key == Key.Return)
                RefreshData();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (!IsReadOnly)
            {
                var isApply = e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                // если нас попросили применить текущее значение
                if (isApply && PopupContentGridControl != null)
                {
                    var item = PopupContentGridControl.GetFocusedRow();
                    if (item != null)
                    {
                        var properties = TypeDescriptor.GetProperties(item);
                        var property = properties[ValueMember];
                        if (property != null)
                            EditValue = property.GetValue(item);
                    }
                    ClosePopup();
                    e.Handled = true;
                    return;
                }

                var isPast = e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

                // при нажатии Enter или Ctrl+V выставляем фильтр грида и обновляем данные
                if (e.Key == Key.Enter || e.Key == Key.Return || isPast)
                {
                    // начали серьезно работать
                    IsSimpleMode = false;

                    var displayValue = isPast ? Clipboard.GetText() : DisplayText;

                    // если уже открывали "выпадайку", то можно сразу получать данные
                    if (PopupContentGridControl != null)
                    {
                        if (RefreshDataByDisplayMemverValue(displayValue))
                        {
                            //Совместимость с версией 15.2
                            //if (isPast)
                            //{
                            //    Text = null;
                            //    IsPopupOpen = true;
                            //    e.Handled = true;
                            //}

                            // елси нажали Enter при открытом списке, значит ищем
                            if ((e.Key == Key.Enter || e.Key == Key.Return || isApply) && IsPopupOpen)
                                e.Handled = true;
                        }
                    }
                    else // если не открывали - уходим на отложенный запуск фильтрации
                    {
                        // сбрасываем автопоиск в гриде, чтобы увидеть найденое значение. почему не работает с ним - загадка :(
                        _previosIncrementalFiltering = IncrementalFiltering.HasValue;
                        IncrementalFiltering = false;
                        // запоминаем значения для фильтрации
                        _displayValueForFilteringAfterPopupOpened = displayValue;
                    }
                }
            }

            base.OnPreviewKeyDown(e);
        }

        private bool RefreshDataByDisplayMemverValue(string filter)
        {
            var valColumn = PopupContentGridControl.Columns[LookupInfo.ValueMember];
            var dispColumn = PopupContentGridControl.Columns[LookupInfo.DisplayMember];
            if (valColumn == null && dispColumn == null)
                return false;

            if (IsDisplayEqualValue() || valColumn == null || dispColumn == null)
            {
                if (dispColumn != null)
                    dispColumn.AutoFilterValue = filter;
                else
                    valColumn.AutoFilterValue = filter;
            }
            else
            {
                PopupContentGridControl.FilterString = string.Format("Contains({0}, '{2}') OR Contains({1}, '{2}')",
                    LookupInfo.DisplayMember, LookupInfo.ValueMember, filter);
            }

            RefreshData();
            
            //Совместимость с версией 15.2
            //ItemsProvider.DoRefresh();

            return true;
        }

        protected override bool IsClosePopupWithCancelGesture(Key key, ModifierKeys modifiers)
        {
            //определяем, что закрываем горячими клавишами
            PopupCloseMode = (IsPopupOpen && IsTogglePopupOpenGesture(key, modifiers))
                ? PopupCloseMode.Cancel
                : PopupCloseMode.Normal;
            return base.IsClosePopupWithCancelGesture(key, modifiers);
        }

        private void RaiseCanExecuteChanged()
        {
            _openReferenceWindowCommand.RaiseCanExecuteChanged();
        }

        protected bool CanShowWindow()
        {
            return !IsInWaiting;
        }

        protected virtual async void OnShowWindow()
        {
            if (!CanShowWindow())
                return;

            if (IsSimpleMode && (ItemsSource == null || (ItemsSource is IList && ((IList) ItemsSource).Count == 0)) &&
                EditValue != null)
            {
                await RefreshData(false);
            }

            using (var window = CreateWindow())
            {
                if (window.ShowDialog() == true && !IsReadOnly)
                {
                    var model = window.DataContext as IObjectListViewModel;
                    if (model != null && model.SelectedItem != null)
                    {
                        var pd = TypeDescriptor.GetProperties(LookupInfo.ItemType);
                        var property = pd.Find(LookupInfo.ValueMember, true);
                        if (property != null)
                        {
                            // чтобы не получать еще раз данные - берем из из модели
                            InItemSourceChanging = true; //Значение InItemSourceChanging будет изменено на false в SetItemsSource
                            IsSimpleMode = false;
                            if (string.IsNullOrEmpty(DisplayMember) || string.IsNullOrEmpty(ValueMember))
                                SetLookupDisplyProperties();
                            EditValue = property.GetValue(model.SelectedItem);
                            SetItemsSource(model.GetSource());
                        }
                    }
                }
            }
        }

        protected virtual CustomLookUpOptPopupContent CreateWindow()
        {
            var destType = typeof(IListViewModel<>).MakeGenericType(LookupInfo.ItemType);
            var model = (IObjectListViewModel)IoC.Instance.Resolve(destType, null);
            model.Mode = ObjectListMode.LookUpList3Points;
            model.AllowAddNew = true;
            model.InitializeMenus();
            //model.IsFilterVisible = true;

            var modelCapt = model as PanelViewModelBase;
            if (modelCapt != null)
            {
                modelCapt.SetPanelCaptionPrefix(DataContext.GetType());
                modelCapt.IsActive = true;
            }

            if (EditValue != null)
            {
                model.ValueMember = GetValueMember();
                model.EditValue = EditValue;
            }

            // выставляем ограничения на кол-во строк
            if (model.CustomFilters != null)
            {
                model.CustomFilters.MaxRowCount = MaxFetchItemsCount;
                model.CustomFilters.FilterExpression = "NONE";
                model.CustomFilters.SqlFilterExpression = FilterInternal;

                if (!string.IsNullOrEmpty(FilterInternal)) //Внимание. Метод зависит от наличия меню - model.InitializeMenus
                    model.ChangeImageFilter(false);
            }

            // если у нас уже нафильтровано какое-то кол-во значений - отдаем их в форму
            if (ItemsSource != null)
            {
                var items = ItemsSource.Clone();
                if (items is IList)
                {
                    foreach (var item in ((IList)items).OfType<EditableBusinessObject>())
                    {
                        item.AcceptChanges();
                    }
                }

                model.SetSource(items);
                //model.CustomFilters.FilterExpression = FilterInternal;
            }
            else // иначе лезем в БД за данными для "..."
            {
                model.ApplyFilter();
            }

            var result = new CustomLookUpOptPopupContent
            {
                DataContext = model,
            };

            if (result.Owner == null && Application.Current.MainWindow.IsActive)
                result.Owner = Application.Current.MainWindow;
            return result;
        }

        protected string GetValueMember()
        {
            if (!string.IsNullOrEmpty(ValueMember))
                return ValueMember;
            if (LookupInfo == null)
                return null;
            return LookupInfo.ValueMember;
        }

        private void OnEditButtonValidation()
        {
            foreach (var button in Buttons)
            {
                OnEditButtonValidation(button, Equals(button.Tag, Button3PointName) || !IsReadOnly);
            }
            RaiseCanExecuteChanged();
        }

        private void OnEditButtonValidation(ButtonInfoBase button, bool isvisible)
        {
            if (button == null)
                return;

            button.IsEnabled = isvisible;
            button.Visibility = isvisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // если явно не задали виртуальное поле - пытаемся отыскать его самостоятельно
            if (DataContext != null && string.IsNullOrEmpty(VirtualFieldName))
            {
                var be = BindingOperations.GetBindingExpression(this, EditValueProperty);
                if (be != null)
                {
                    var properties = TypeDescriptor.GetProperties(DataContext);
                    var property = properties.Find(be.ParentBinding.Path.Path, true);
                    if (property != null)
                    {
                        var virtualAttr = property.Attributes[typeof(LinkToVirtualFieldAttribute)] as LinkToVirtualFieldAttribute;
                        if (virtualAttr != null && !string.IsNullOrEmpty(virtualAttr.VirtualFieldName))
                            VirtualFieldName = virtualAttr.VirtualFieldName;
                    }
                }
            }

            UnSubscribeDataContext(e.OldValue);
            var filter = string.Empty;

            Action<object> findFilterHandler = data =>
            {
                if (data == null)
                    return;

                // учим работать с несколькими пропертями
                if (_varFilter != null)
                {
                    var sb = new StringBuilder();
                    foreach (var key in _varFilter)
                    {
                        var add = UpdateFilter(key.Key, data);
                        if(!string.IsNullOrEmpty(add))
                            sb.Append(sb.Length > 0 ? " AND " + add : add);
                    }

                    if (sb.Length > 0)
                        filter = string.IsNullOrEmpty(filter) ? sb.ToString() : filter + " AND " + sb.ToString();
                }
            };

            if (_varFilter != null)
            {
                findFilterHandler(ParentViewModelSource);
                findFilterHandler(DataContext);
            }
            SetFilter(string.IsNullOrEmpty(filter) ? null : filter);

            SubscribeDataContext(e.NewValue);
        }

        private void OnDataContextNotifyPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //Если вызвали из другого потока - выходим
            if (!Dispatcher.CheckAccess())
                return;

            // IsDitry вызывал передергиванием всех лукапов и сброс varFilter-ов в null 
            if (!propertyChangedEventArgs.PropertyName.Equals(EditableBusinessObject.IsDirtyPropertyName))
            {
                // учим работать с несколькими пропертями
                if (_varFilter != null)
                {
                    var filter = new StringBuilder();
                    foreach (var key in _varFilter)
                    {
                        var add = UpdateFilter(key.Key, DataContext);
                        if(!string.IsNullOrEmpty(add))
                            filter.Append(filter.Length > 0 ? " AND " + add : add);
                    }
                    if (ParentViewModelSource != null)
                    {
                        var filterParent = new StringBuilder();
                        foreach (var key in _varFilter)
                        {
                            var add = UpdateFilter(key.Key, ParentViewModelSource);
                            if (!string.IsNullOrEmpty(add))
                                filterParent.Append(filterParent.Length > 0 ? " AND " + add : add);
                        }
                        if (filterParent.Length > 0)
                            filter = (filter.Length > 0) ? filter.Append(" AND " + filterParent) : filterParent;
                    }
                    if (filter.Length > 0)
                        SetFilter(filter.ToString());
                }
            }
        }

        private void SubscribeDataContext(object dataContext)
        {
            var iNotifyPropertyChanged = dataContext as INotifyPropertyChanged;
            if (iNotifyPropertyChanged != null)
                iNotifyPropertyChanged.PropertyChanged += OnDataContextNotifyPropertyChanged;
        }

        private void UnSubscribeDataContext(object dataContext)
        {
            var iNotifyPropertyChanged = dataContext as INotifyPropertyChanged;
            if (iNotifyPropertyChanged != null)
                iNotifyPropertyChanged.PropertyChanged -= OnDataContextNotifyPropertyChanged;
        }

        private string UpdateFilter(string key, object data)
        {
            if (_varFilter == null || data == null || string.IsNullOrEmpty(key))
                return null;

            var keyinternal = LookupHelper.GetStringValue(key);
            if (_varFilter.ContainsKey(keyinternal))
            {
                string filter = null;
                var propvalue = string.Empty;
                // учим работать с ExpandoObject
                if (data is ExpandoObject)
                {
                    var dict = data as IDictionary<string, object>;
                    if (dict.ContainsKey(key))
                        propvalue = dict[key].To<string>();
                    if (string.IsNullOrEmpty(propvalue))
                        propvalue = LookupHelper.FilterValueNull;
                }
                else if (data is CustomExpandoObject)
                {
                    var dict = ((CustomExpandoObject)data).Members;
                    if (dict.ContainsKey(key))
                        propvalue = dict[key].To<string>();
                    if (string.IsNullOrEmpty(propvalue))
                        propvalue = LookupHelper.FilterValueNull;
                }
                else
                {
                    var prdesc = TypeDescriptor.GetProperties(data);
                    var prop = prdesc.Find(keyinternal, true);
                    if (prop == null)
                        return null;
                    propvalue = LookupHelper.GetStringValue(prop.GetValue(data).To(LookupHelper.FilterValueNull));
                }

                if (string.IsNullOrEmpty(propvalue))
                    return null;

                if (_varFilter[keyinternal].ContainsKey(propvalue)) //проверяем - есть ли фильтр для данного значения
                {
                    filter = string.Format(_varFilter[keyinternal][propvalue], propvalue);
                }
                else if (_varFilter[keyinternal].ContainsKey(LookupHelper.FilterValueAny)) //если нет - есть ли универсальный фильтр (*)
                {
                    filter = string.Format(_varFilter[keyinternal][LookupHelper.FilterValueAny], propvalue);
                }

                return filter;
            }
            return null;
        }

        private void SetFilter(string filter)
        {
            var filter0 = _filter0.GetTrim();
            var filterInternal = string.IsNullOrEmpty(filter) ? filter0 : string.Format("{0}{1}{2}", filter0, string.IsNullOrEmpty(filter0) ? null : " AND ", filter);
            if (!string.IsNullOrEmpty(LookUpCodeEditorFilterExt) && LookUpCodeEditorFilterExt.StartsWith("pkg"))
            {
                _isPkg = true;
            }
            else
            {
                filterInternal += string.IsNullOrEmpty(LookUpCodeEditorFilterExt)
                    ? string.Empty
                    : string.IsNullOrEmpty(filterInternal)
                        ? LookUpCodeEditorFilterExt
                        : " AND " + LookUpCodeEditorFilterExt;
            }
            FilterInternal = filterInternal;
        }

        private void InitializeLookUpCodeEditor()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                throw new DeveloperException("Lookup code is not set.");

            LookupInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);

            // определяем максимально кол-во элементов, которые нужно получать
            MaxFetchItemsCount = LookupInfo.FetchRowCount.HasValue
                ? Convert.ToInt32(LookupInfo.FetchRowCount.Value)
                : (int?)null;

            var filtertxt = LookupInfo.Filter;
            filtertxt += string.IsNullOrEmpty(LookUpCodeEditorVarFilterExt)
                             ? string.Empty
                             : LookUpCodeEditorVarFilterExt;

            // если извне хотят управлять колонками - пусть сам и управляют
            if (LookUpColumnsSource == null)
                LookUpColumnsSource = DataFieldHelper.Instance.GetDataFields(LookupInfo.ItemType, SettingDisplay.LookUp);

            LookupHelper.InitializeVarFilter(filtertxt, out _filter0, out _varFilter);

            // инстанируем
            ManagerInstance = LookupHelper.GetItemSourceManager(LookupInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;

            // получим property - primarykey

            _primaryKeyPropertyName = WMSBusinessObject.GetPrimaryKeyPropertyName(LookupInfo.ItemType);
        }

        private void OnChangedManagerInstance(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (!IsSimpleMode)
                RefreshData();
        }

        protected void SetItemsSource(object source, bool? hasMoreData = null)
        {
            try
            {
                InItemSourceChanging = true;
                var editvalue = EditValue;
                ItemsSource = source;

                //Восстанавливаем справедливость
                if (editvalue != null && EditValue == null)
                    EditValue = editvalue;
            }
            finally
            {
                InItemSourceChanging = false;
            }

            var cln = source as IList;
            // если не передали - вычисляем сами
            HasMoreData = hasMoreData ?? MaxFetchItemsCount.HasValue && cln != null && cln.Count >= MaxFetchItemsCount.Value;
        }

        public async Task<bool> RefreshData(bool? setSimpleModeTo = null)
        {
            //Если вызвали из другого потока - выходим
            if (!Dispatcher.CheckAccess())
                return false;

            var settings = Settings as CustomBaseLookupEditSetting;
            if (settings != null && settings.NotLoadDataFromEditor)
                return false;

            try
            {
                if (setSimpleModeTo.HasValue)
                    IsSimpleMode = setSimpleModeTo.Value;

                // уже находимся в процессе обновления - выходим
                if (InItemSourceChanging)
                    return false;

                WaitStart();

                RaiseCanExecuteChanged();

                var items = await GetDataAsync();
                if (items == null)
                    SetItemsSource(null, false);
                else
                {
                    var clnType = typeof(ObservableRangeCollection<>).MakeGenericType(LookupInfo.ItemType);
                    var cln = (IList)Activator.CreateInstance(clnType);
                    foreach (var item in items)
                        cln.Add(item);

                    SetItemsSource(cln);
                }

                // выставляем признак наличия еще данных в БД
                var custGrid = PopupContentGridControl as CustomGridControl;
                if (custGrid != null)
                    custGrid.TotalRowItemFilteredSymbolIsVisible = HasMoreData;

                // выставляем значения только после получения данных
                SetLookupDisplyProperties();

                //Если есть EditValue пытаемся выделить соответствующую запись в GridControl
                if (ItemsProvider != null && EditValue != null && SelectedItem == null)
                {
                    var correctedEditValue = GetCorrectedEditValue(EditValue);
                    SelectedItem = ItemsProvider.GetItem(correctedEditValue, null);
                }

                return true;
            }
            finally
            {
                WaitStop();
                RaiseCanExecuteChanged();
            }
        }

        protected virtual void SetLookupDisplyProperties()
        {
            DisplayMember = LookupInfo.DisplayMember;
            ValueMember = LookupInfo.ValueMember;
        }

        protected virtual async Task<IEnumerable<object>> GetDataAsync()
        {
            var filter = GetDataFilter();
            if (IsSimpleMode && (CanUseVirtualField() || string.IsNullOrEmpty(filter)))
                return null;

            return await Task.Factory.StartNew(() =>
            {
                var mgr = LookupHelper.GetItemSourceManager(LookupInfo);
                return mgr.GetFiltered(filter, GetModeEnum.Partial);
            });
        }

        /// <summary>
        /// Признак того, что Lookup может работать в режиме отображения вритуального поля вместо запроса данных
        /// </summary>
        protected bool CanUseVirtualField()
        {
            if (!IsSimpleMode || string.IsNullOrEmpty(VirtualFieldName))
                return false;

            return !IsNewMode();
        }

        private void WaitStart()
        {
            if (IsInWaiting)
                return;

            IsInWaiting = true;

            _previosBrush = Background;
            Background = WaitingBackgroundColor;

            if (PopupContentGridControl != null)
                PopupContentGridControl.ShowLoadingPanel = true;
        }

        private void WaitStop()
        {
            Background = _previosBrush;

            if (PopupContentGridControl != null)
                PopupContentGridControl.ShowLoadingPanel = false;

            IsInWaiting = false;
        }

        protected virtual void OnCustomEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            // если уже получаем данные или очищаем значение, то лезть за данными не нужно
            if (IsInWaiting || e.NewValue == null)
                return;

            if (ValueMemberType == null && LookupInfo != null)
            {
                var desc = TypeDescriptor.GetProperties(LookupInfo.ItemType);
                ValueMemberType = desc[LookupInfo.ValueMember].PropertyType;
            }

            // если виратуальное поле не привязано или мы только создаем объект - делаем запрос за этой строчкой
            if (IsSimpleMode && (IsNewMode() || !CanUseVirtualField()))
                RefreshData();

            if (ItemsProvider != null && EditValue != null && !string.IsNullOrEmpty(VirtualFieldName))
            {
                var be = BindingOperations.GetBindingExpression(this, EditValueProperty);
                if (be != null)
                {
                    var obj = DataContext as WMSBusinessObject;
                    if (obj != null && obj.GetPropertyIsDirty(be.ParentBinding.Path.Path))
                    {
                        // INFO: выставляем виртуальное поле по параметру лукапа
                        // TODO: необходимо обновлять все зависящие виртуальные поля
                        var text = ItemsProvider.GetDisplayValueByEditValue(EditValue, null);
                        try
                        {
                            if (!Equals(obj.GetProperty(VirtualFieldName), text))
                                obj.SetProperty(VirtualFieldName, text);
                        }
                        catch
                        {
                            // не удалось присвоить - очищаем
                            try
                            {
                                obj.SetProperty(VirtualFieldName, null);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        protected virtual object GetCorrectedEditValue(object editValue)
        {
            // если тип поля и тип ключа лукапа не совпадают. то сами приводим к нужному
            if (editValue != null && ValueMemberType != null && !ValueMemberType.IsInstanceOfType(editValue))
                return SerializationHelper.ConvertToTrueType(editValue, ValueMemberType);
            return editValue;
        }

        protected bool IsNewMode()
        {
            var n = DataContext as IIsNew;
            if (n == null)
                return false;

            return n.IsNew;
        }

        /// <summary> Сборка результирующего фильтра </summary>
        protected string GetDataFilter()
        {
            if (_isPkg)
                return LookUpCodeEditorFilterExt;

            // берем общий фильтр
            var mainFilter = FilterHelper.SurroundWithBrackets(FilterInternal);

            // фильтр грида
            string gridFilter = null;
            if (!IsSimpleMode && PopupContentGridControl != null)
            {
                gridFilter = FilterHelper.ConvertGridFilter(PopupContentGridControl.FilterString, LookupInfo.ItemType);
                gridFilter = FilterHelper.SurroundWithBrackets(gridFilter);
            }

            // фильтр на текущий элемент, чтобы он обязательно был в выборке
            string equalEditValueFilter = null;
            string nonEqualEditValueFilter = null;

            // проверяем строковое значение (может быть пусто, вместо null)
            var correctedEditValue = GetCorrectedEditValue(EditValue);
            if (!string.IsNullOrEmpty(correctedEditValue.To<string>()) && !string.IsNullOrEmpty(LookupInfo.ValueMember))
            {
                var propertyName = SourceNameHelper.Instance.GetPropertySourceName(LookupInfo.ItemType, LookupInfo.ValueMember);

                equalEditValueFilter = FilterHelper.ConstructEquals(propertyName, correctedEditValue);
                equalEditValueFilter = FilterHelper.SurroundWithBrackets(equalEditValueFilter);

                nonEqualEditValueFilter = FilterHelper.ConstructNonEquals(propertyName, correctedEditValue);
                nonEqualEditValueFilter = FilterHelper.SurroundWithBrackets(nonEqualEditValueFilter);
            }
            
            string filter;
            // ограничение на максимальное кол-во строк
            // если нам не нужно получать данные - запрашиваем только текущий (один) элемент
            var fetchFilter = FilterHelper.GetFetchCountFilter(1); 
            if (IsSimpleMode)
            {
                filter = FilterHelper.And(mainFilter, gridFilter, equalEditValueFilter, fetchFilter);
            }
            else
            {
                var isNullOrEmptyEqualEditValueFilter = string.IsNullOrEmpty(equalEditValueFilter);
                if (MaxFetchItemsCount.HasValue)
                    fetchFilter = FilterHelper.GetFetchCountFilter(MaxFetchItemsCount.Value - (isNullOrEmptyEqualEditValueFilter ? 0 : 1));
                filter = FilterHelper.And(mainFilter, gridFilter, fetchFilter);
                if (!isNullOrEmptyEqualEditValueFilter)
                {
                    // если фильруем не по PK, то обязательно добавляем 
                    if (_primaryKeyPropertyName != LookupInfo.ValueMember)
                       equalEditValueFilter = FilterHelper.And(equalEditValueFilter, filter);

                    // исключаем текущее значение из основного фильтра
                    filter = FilterHelper.And(filter, nonEqualEditValueFilter);

                    // добавляем текущее значение отдельным фильтром
                    filter = equalEditValueFilter + FilterHelper.Semicolon + filter;
                }
            }

            return filter;
        }
        #endregion .  Methods  .
    }

    internal class CustomLookUpEditStrategy : LookUpEditStrategy
    {
        public CustomLookUpEditStrategy(LookUpEdit editor)
            : base(editor)
        {
        }

        /// <summary>
        /// Опредяляет начало процесса PerformNullInput.
        /// </summary>
        public bool IsProcessNullInput { get; private set; }

        protected override void PerformNullInput()
        {
            IsProcessNullInput = true;
            try
            {
                base.PerformNullInput();
            }
            finally
            {
                IsProcessNullInput = false;
            }
        }

        //Совместимость с версией 15.2
        protected override void ProcessChangeText(ChangeTextItem item)
        {
            var editor = Editor as CustomLookUpEdit;
            if (editor == null || editor.UseNativeAutoCompleteSelection)
            {
                base.ProcessChangeText(item);
                return;
            }

            //var text = item.Text;
            //bool updateAutoCompleteSelection = item.UpdateAutoCompleteSelection;
            //this.UpdateAutoSearchBeforeValidate(item);
            //int index = this.FindItemIndexByText(text, ((LookUpEdit)Editor).AutoComplete);
            //var editValue = this.CreateEditableItem(index, item);
            var editValue = CreateEditableItem(-1, item);
            base.ValueContainer.SetEditValue(editValue, UpdateEditorSource.TextInput);
            //this.UpdateAutoSearchAfterValidate(item);
            UpdateDisplayText();
            //this.UpdateAutoSearchSelection(updateAutoCompleteSelection);
            ShowImmediatePopup();
        }
    }
}
