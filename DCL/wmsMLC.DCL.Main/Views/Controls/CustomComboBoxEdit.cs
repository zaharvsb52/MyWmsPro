using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Editors;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomComboBoxEdit : ComboBoxEdit, IDisposable
    {
        #region .  Fields  .
        private string _filter0;
        private IBaseManager _managerInstance;
        private string _lookUpCodeEditor;
        private string _filterInternal;
        private string _displayMember;
        private object _editValue;
        private bool _isPkg;

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
            _isPkg = false;
            AutoComplete = true;
            ImmediatePopup = true;
            AllowSpinOnMouseWheel = false;

            _filterInternal = "  ";
            DataContextChanged += OnDataContextChanged;
            EditValueChanged += OnEditValueChanged;
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

        private void OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            _editValue = e.NewValue;
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

        private LookupInfo LookUpInfo { get; set; }

        public string LookUpCodeEditor
        {
            get { return _lookUpCodeEditor; }
            set
            {
                if (_lookUpCodeEditor == value)
                    return;

                _lookUpCodeEditor = value;
                InitializeLookUpCodeEditor();
            }
        }

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
            if (!string.IsNullOrEmpty(LookUpCodeEditorFilterExt) && LookUpCodeEditorFilterExt.StartsWith("pkg"))
            {
                _isPkg = true;
            }
            else
            {
                filterInternal += string.IsNullOrEmpty(LookUpCodeEditorFilterExt)
                    ? string.Empty
                    : string.IsNullOrEmpty(FilterInternal)
                        ? LookUpCodeEditorFilterExt
                        : " AND " + LookUpCodeEditorFilterExt;
            }

            FilterInternal = filterInternal;
        }

        private void InitializeLookUpCodeEditor()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                throw new DeveloperException("Lookup code is not set.");

            LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
            _displayMember = LookUpInfo.DisplayMember;
            if (Settings != null)
                NullText = Settings.NullText;

            var filtertxt = LookUpInfo.Filter;
            filtertxt += string.IsNullOrEmpty(LookUpCodeEditorVarFilterExt)
                             ? string.Empty
                             : LookUpCodeEditorVarFilterExt;

            LookupHelper.InitializeVarFilter(filtertxt, out _filter0, out _varFilter);

            ManagerInstance = LookupHelper.GetItemSourceManager(LookUpInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;

            //Если контрол запускается без сеттингов (т.е. не из GridControl'а) то получаем ItemsSource
            //if (!IsSettings)
            //{
            //    var oldfilter = FilterInternal;
            //    SetFilter(null);
            //    if (oldfilter == FilterInternal)
            //        RefreshData();
            //}
        }

        private void ManagerInstanceOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshData();
        }

        private new async void RefreshData()
        {
            if (ManagerInstance == null)
                return;

            var trueNullText = NullText;
            try
            {
                NullText = StringResources.Wait;
                var source = await GetDataAsync();
                ItemsSource = LookupHelper.GetComboItemsSource(source, _displayMember);
            }
            finally
            {
                NullText = trueNullText;
            }
        }
        private async Task<IEnumerable<object>> GetDataAsync()
        {
            return await Task.Factory.StartNew(() =>
                {
                    var filter = (_isPkg != true) ? FilterInternal : LookUpCodeEditorFilterExt;
                    return ManagerInstance.GetFiltered(filter, GetModeEnum.Partial);
                });
        }

        protected override string GetDisplayText(object editValue, bool applyFormatting)
        {
            if (!IsSettings)
                return base.GetDisplayText(editValue, applyFormatting);

            if (Settings == null)
                return base.GetDisplayText(editValue, applyFormatting);

            //            if (settings.IsInLoading && ItemsSource == null)
            //                return StringResources.Wait;
            //
            //            if (!settings.AllowAddNewValue)
            //                return base.GetDisplayText(editValue, applyFormatting);

            var editvalue = editValue ?? _editValue;
            if (ItemsSource != null && !string.IsNullOrEmpty(editvalue.To<string>()))
            {
                //_editValue = null;
                var displayvalue = ItemsProvider.GetDisplayValueByEditValue(editvalue, null);
                return displayvalue.To(" ");
            }

            return base.GetDisplayText(editValue, applyFormatting);
        }

        protected override string GetCustomDisplayText(object editValue, string displayText)
        {
            var res = base.GetCustomDisplayText(editValue, displayText);
            if (editValue != null && string.IsNullOrEmpty(displayText))
            {
                return editValue.ToString();
            }
            return res;
        }
    }
}
