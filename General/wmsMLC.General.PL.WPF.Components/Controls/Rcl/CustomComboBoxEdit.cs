using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Editors;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomComboBoxEdit : ComboBoxEdit, IDisposable
    {
        #region .  Fields  .
        private string _filter0;
        private IBaseManager _managerInstance;
        private string _filterInternal;

        /// <summary>
        /// VarFilter. Формат: DefaultFilter [$PropertyName1,(Value11, Filter11),(Value12,Filter12),...],[$PropertyName2,(Value21, Filter21),(Value22,Filter22),...],...
        ///, где
        ///DefaultFilter - Основной фильтр (если есть)
        ///$PropertyName1 - имя свойства сущности,
        ///Value11 - значение данного свойства,
        ///Filter11 - фильтр для данного значения.
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _varFilter;
        #endregion .  Fields  .

        public CustomComboBoxEdit()
        {
            AutoComplete = true;
            ImmediatePopup = true;
            AllowSpinOnMouseWheel = false;

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoad;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoad;
            InitializeLookUpCodeEditor();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsSettings)
                return;

            UnSubscribeDataContext(e.OldValue);

            var isneedrefreshdata = true;
            Action<object> findFilterHandler = data =>
            {
                if (data == null)
                    return;
                isneedrefreshdata &= !_varFilter.Keys.Any(key => UpdateFilter(key, data));
            };

            if (_varFilter != null)
            {
                findFilterHandler(ParentViewModelSource);
                findFilterHandler(DataContext);
            }
            if (isneedrefreshdata)
            {
                SetFilter(null);
            }

            SubscribeDataContext(e.NewValue);
        }

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~CustomComboBoxEdit()
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
        /// Освобождение ресурсов.
        /// </summary>
        /// <param name="disposing">False - если требуется освободить только UnManaged ресурсы, True - если еще и Managed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                EditValueChanged -= OnEditValueChanged;
                DataContextChanged -= OnDataContextChanged;
                UnSubscribeDataContext(DataContext);
                if (ManagerInstance != null)
                {
                    ManagerInstance.Changed -= ManagerInstanceOnChanged;
                    ManagerInstance.Dispose();
                    ManagerInstance = null;
                }
                ItemsSource = null;
            }
            // unmanaged objects and resources
        }
        #endregion

        #region .  Properties  .
        public object ParentViewModelSource { get; set; }

        protected LookupInfo LookUpInfo { get; set; }
        public string LookUpCodeEditor { get; set; }
        public string LookUpCodeEditorFilterExt { get; set; }
        public string LookUpCodeEditorVarFilterExt { get; set; }
        protected internal bool IsSettings { get; set; }

        private string FilterInternal
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
                RefreshData();
            }
        }

        private IBaseManager ManagerInstance
        {
            get { return _managerInstance; }
            set
            {
                if (_managerInstance != null)
                    _managerInstance.Changed -= ManagerInstanceOnChanged;
                //ManagerChangedEventManager.RemoveListener(_managerInstance, this);

                _managerInstance = value;

                if (_managerInstance != null)
                    _managerInstance.Changed += ManagerInstanceOnChanged;
                //ManagerChangedEventManager.AddListener(_managerInstance, this);
            }
        }

        #endregion

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

        private void OnDataContextNotifyPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateFilter(propertyChangedEventArgs.PropertyName, DataContext);
        }

        private bool UpdateFilter(string key, object data)
        {
            if (_varFilter == null || data == null || string.IsNullOrEmpty(key))
                return false;

            var keyinternal = LookupHelper.GetStringValue(key);
            if (_varFilter.ContainsKey(keyinternal))
            {
                string filter = null;
                var prdesc = TypeDescriptor.GetProperties(data);
                var prop = prdesc.Find(keyinternal, true);
                if (prop == null)
                    return false;

                var propvalue = LookupHelper.GetStringValue(prop.GetValue(data).To(LookupHelper.FilterValueNull));

                if (_varFilter[keyinternal].ContainsKey(propvalue)) //проверяем - есть ли фильтр для данного значения
                {
                    filter = string.Format(_varFilter[keyinternal][propvalue], propvalue);
                }
                else if (_varFilter[keyinternal].ContainsKey(LookupHelper.FilterValueAny)) //если нет - есть ли универсальный фильтр (*)
                {
                    filter = string.Format(_varFilter[keyinternal][LookupHelper.FilterValueAny], propvalue);
                }

                if (filter == null)
                    return false;

                SetFilter(filter);
                return true;
            }
            return false;
        }

        private void SetFilter(string filter)
        {
            var filter0 = _filter0.GetTrim();
            var filterInternal = string.IsNullOrEmpty(filter)
                ? filter0
                : string.Format("{0}{1}{2}", filter0, string.IsNullOrEmpty(filter0) ? null : " AND ", filter);
            filterInternal += string.IsNullOrEmpty(LookUpCodeEditorFilterExt)
                                  ? string.Empty
                                  : string.IsNullOrEmpty(FilterInternal) ? LookUpCodeEditorFilterExt : " AND " + LookUpCodeEditorFilterExt;
            FilterInternal = filterInternal;
        }

        private void InitializeLookUpCodeEditor()
        {
            if (IsSettings)
                return;

            if (string.IsNullOrEmpty(LookUpCodeEditor))
                throw new DeveloperException("Lookup code is not set.");

            LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
            ValueMember = LookUpInfo.ValueMember;
            DisplayMember = LookUpInfo.DisplayMember;
            var typedSettings = Settings as CustomCommonLookupSettings;
            if (typedSettings != null)
                NullText = typedSettings.NullText;

            var filtertxt = LookUpInfo.Filter;
            filtertxt += string.IsNullOrEmpty(LookUpCodeEditorVarFilterExt)
                             ? string.Empty
                             : LookUpCodeEditorVarFilterExt;

            LookupHelper.InitializeVarFilter(filtertxt, out _filter0, out _varFilter);

            ManagerInstance = LookupHelper.GetItemSourceManager(LookUpInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;

            //Если контрол запускается без сеттингов IsSettings = false (т.е. не из GridControl'а) то получаем ItemsSource
            var oldfilter = FilterInternal;
            SetFilter(null);
            if (oldfilter == FilterInternal)
                RefreshData();
        }

        private void ManagerInstanceOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshData();
        }

        private async void RefreshData()
        {
            if (ManagerInstance == null)
                return;

            var trueNullText = NullText;
            try
            {
                NullText = Properties.Resources.Wait;
                ItemsSource = await GetDataAsync(); 
            }
            finally
            {
                NullText = trueNullText;
            }
        }

        private async Task<IEnumerable<object>> GetDataAsync()
        {
            return await Task.Factory.StartNew(() => ManagerInstance.GetFiltered(FilterInternal, GetModeEnum.Partial));
        }
    }
}
