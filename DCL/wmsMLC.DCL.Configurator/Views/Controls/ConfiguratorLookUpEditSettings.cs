using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.LookUp;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Configurator.Views.Controls
{
    public class ConfiguratorLookUpEditSettings : LookUpEditSettings, IDisposable, IConfiguratorSettingsBase
    {
        private IBaseManager _managerInstance;

        static ConfiguratorLookUpEditSettings()
        {
            //EditorSettingsProvider.Default.RegisterUserEditor(typeof(ConfiguratorLookUpEdit), typeof(ConfiguratorLookUpEditSettings),
            //    () => new ConfiguratorLookUpEdit(),
            //    () => new ConfiguratorLookUpEditSettings());

            EditorSettingsProvider.Default.RegisterUserEditor2(typeof(ConfiguratorLookUpEdit),
             typeof(ConfiguratorLookUpEditSettings),
             optimized => optimized ? (IBaseEdit)new InplaceBaseEdit() : new ConfiguratorLookUpEdit(),
             () => new ConfiguratorLookUpEditSettings());
        }

        public ConfiguratorLookUpEditSettings()
        {
            Loaded += OnLoaded;
        }

        #region .  Properties  .
        public string LookUpCodeEditor
        {
            get { return (string)GetValue(LookUpCodeEditorProperty); }
            set { SetValue(LookUpCodeEditorProperty, value); }
        }

        public static readonly DependencyProperty LookUpCodeEditorProperty = DependencyProperty.Register("LookUpCodeEditor", typeof(string), typeof(ConfiguratorLookUpEditSettings), new PropertyMetadata(OnLookUpCodeEditorPropertyChanged));

        public string LookUpCodeEditorFilterExt
        {
            get { return (string)GetValue(LookUpCodeEditorFilterExtProperty); }
            set { SetValue(LookUpCodeEditorFilterExtProperty, value); }
        }

        public static readonly DependencyProperty LookUpCodeEditorFilterExtProperty = DependencyProperty.Register("LookUpCodeEditorFilterExt", typeof(string), typeof(ConfiguratorLookUpEditSettings));

        public CriteriaOperator LookupFilterCriteria
        {
            get { return (CriteriaOperator)GetValue(LookupFilterCriteriaProperty); }
            set { SetValue(LookupFilterCriteriaProperty, value); }
        }

        public static readonly DependencyProperty LookupFilterCriteriaProperty = DependencyProperty.Register("LookupFilterCriteria", typeof(CriteriaOperator), typeof(ConfiguratorLookUpEditSettings));

        public ObservableCollection<DataField> LookUpColumnsSource { get; set; }

        private LookupInfo LookUpInfo { get; set; }

        protected string Filter0 { get; set; }
        protected string FilterInternal { get; set; }

        private IBaseManager ManagerInstance
        {
            get { return _managerInstance; }
            set
            {
                if (_managerInstance != null)
                    _managerInstance.Changed -= ManagerInstanceOnChanged;

                _managerInstance = value;

                if (_managerInstance != null)
                    _managerInstance.Changed += ManagerInstanceOnChanged;
            }
        }

        public bool IsInLoading { get; private set; }

        private Type _itemType;
        public Type ItemType
        {
            get
            {
                return LookUpInfo == null ? _itemType : LookupHelper.GetItemSourceType(LookUpInfo);
            }
            set { _itemType = value; }
        }

        /// <summary>
        /// Разрешаем после получения данных в ItemsSource обновить данные родительского GridControl'а в случае, когда у GridControl'а существуют фильтры.
        /// </summary>
        public bool AllowParentGridRefreshData { get; set; }
        #endregion .  Properties  .

        #region . Methods .
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            RefreshData();
        }

        private static void OnLookUpCodeEditorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ConfiguratorLookUpEditSettings)d).OnLookUpCodeEditorPropertyChanged();
        }

        private void OnLookUpCodeEditorPropertyChanged()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                return;

            LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
            DisplayMember = LookUpInfo.DisplayMember;
            ValueMember = LookUpInfo.ValueMember;

            // определяем тип данных
            var lookupSourceItemType = LookupHelper.GetItemSourceType(LookUpInfo);

            if (LookUpColumnsSource == null)
                LookUpColumnsSource = DataFieldHelper.Instance.GetDataFields(lookupSourceItemType, SettingDisplay.LookUp);

            ManagerInstance = LookupHelper.GetItemSourceManager(LookUpInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;
            string filter0;
            LookupHelper.InitializeVarFilter(LookUpInfo.Filter, out filter0);
            Filter0 = filter0;
        }

        protected override void OnItemsSourceChanged(object itemsSource)
        {
            if (LookUpColumnsSource == null && ItemType != null)
                LookUpColumnsSource = DataFieldHelper.Instance.GetDataFields(ItemType, SettingDisplay.LookUp);
            base.OnItemsSourceChanged(itemsSource);
        }

        private void GridDataRefresh()
        {
            if (ItemsSource != null && AllowParentGridRefreshData)
            {
                var column = Parent as GridColumn;
                var grid = column?.Parent as GridControl;
                if (grid == null)
                    return;

                if (grid.ItemsSource != null && !string.IsNullOrEmpty(grid.FilterString))
                    grid.RefreshData();
            }
        }

        private void ManagerInstanceOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    //case NotifyCollectionChangedAction.Reset:
                    RefreshData();
                    break;
            }
        }

        protected async void RefreshData()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                return;

            try
            {
                IsInLoading = true;
                var items = await GetDataAsync();
                DispatcherHelper.Invoke((Action)(() =>
                {
                    ItemsSource = items;
                    GridDataRefresh();
                }));
            }
            finally
            {
                IsInLoading = false;
            }
        }

        private async Task<IEnumerable<object>> GetDataAsync()
        {
            SetFilter();
            return await Task.Factory.StartNew(() => ManagerInstance.GetFiltered(FilterInternal, GetModeEnum.Partial));
        }

        private void SetFilter()
        {
            var filters = new List<string> { Filter0, LookUpCodeEditorFilterExt };
            FilterInternal = string.Join(" AND ", filters.Where(p => !string.IsNullOrEmpty(p)));
        }

        protected override void AssignToEditCore(IBaseEdit edit)
        {
            base.AssignToEditCore(edit);
            var editor = edit as ConfiguratorLookUpEdit;
            if (editor != null)
            {
                editor.LookUpColumnsSource = LookUpColumnsSource;
                editor.LookupFilterCriteria = LookupFilterCriteria;
            }
        }

        public void UpdateData()
        {
            RefreshData();
        }

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~ConfiguratorLookUpEditSettings()
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
            _disposed = true;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        /// <param name="disposing">False - если требуется освободить только UnManaged ресурсы, True - если еще и Managed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (ManagerInstance != null)
            {
                ManagerInstance.AllowMonitorChangesInOtherInsances = false;
                ManagerInstance = null;
                LookUpInfo = null;
            }
        }
        #endregion .  Finalize & Dispose  .
        #endregion . Methods .
    }

    internal interface IConfiguratorSettingsBase
    {
        CriteriaOperator LookupFilterCriteria { get; set; }
    }
}
