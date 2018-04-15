using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Configurator.Views.Controls
{
    public class ConfiguratorComboBoxEditSettings : ComboBoxEditSettings, IDisposable, IConfiguratorSettingsBase
    {
        private IBaseManager _managerInstance;

        static ConfiguratorComboBoxEditSettings()
        {
            //EditorSettingsProvider.Default.RegisterUserEditor(typeof(ConfiguratorComboBoxEdit), typeof(ConfiguratorComboBoxEditSettings),
            //    () => new ConfiguratorComboBoxEdit(),
            //    () => new ConfiguratorComboBoxEditSettings());

            EditorSettingsProvider.Default.RegisterUserEditor2(typeof(ConfiguratorComboBoxEdit),
              typeof(ConfiguratorComboBoxEditSettings),
              optimized => optimized ? (IBaseEdit)new InplaceBaseEdit() : new ConfiguratorComboBoxEdit(),
              () => new ConfiguratorComboBoxEditSettings());
        }

        public ConfiguratorComboBoxEditSettings()
        {
            Loaded += OnLoaded;
        }

        #region .  Properties  .
        public string LookUpCodeEditor
        {
            get { return (string)GetValue(LookUpCodeEditorProperty); }
            set { SetValue(LookUpCodeEditorProperty, value); }
        }

        public static readonly DependencyProperty LookUpCodeEditorProperty = DependencyProperty.Register("LookUpCodeEditor", typeof(string), typeof(ConfiguratorComboBoxEditSettings), new PropertyMetadata(OnLookUpCodeEditorPropertyChanged));

        public string LookUpCodeEditorFilterExt
        {
            get { return (string)GetValue(LookUpCodeEditorFilterExtProperty); }
            set { SetValue(LookUpCodeEditorFilterExtProperty, value); }
        }

        public static readonly DependencyProperty LookUpCodeEditorFilterExtProperty = DependencyProperty.Register("LookUpCodeEditorFilterExt", typeof(string), typeof(ConfiguratorComboBoxEditSettings));

        public CriteriaOperator LookupFilterCriteria
        {
            get { return (CriteriaOperator)GetValue(LookupFilterCriteriaProperty); }
            set { SetValue(LookupFilterCriteriaProperty, value); }
        }

        public static readonly DependencyProperty LookupFilterCriteriaProperty = DependencyProperty.Register("LookupFilterCriteria", typeof(CriteriaOperator), typeof(ConfiguratorComboBoxEditSettings));

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

        public object[] InsertedItems { get; set; }

        public Type ItemType
        {
            get
            {
                return LookUpInfo == null ? null : LookupHelper.GetItemSourceType(LookUpInfo);
            }
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
            ((ConfiguratorComboBoxEditSettings)d).OnLookUpCodeEditorPropertyChanged();
        }

        private void OnLookUpCodeEditorPropertyChanged()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                return;

            LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
            DisplayMember = LookUpInfo.DisplayMember;
            ValueMember = LookUpInfo.ValueMember;

            ManagerInstance = LookupHelper.GetItemSourceManager(LookUpInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;
            string filter0;
            LookupHelper.InitializeVarFilter(LookUpInfo.Filter, out filter0);
            Filter0 = filter0;
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

                if (InsertedItems != null && InsertedItems.Any())
                {
                    var itemsSource = items as IList;
                    if (itemsSource != null)
                    {
                        foreach (var p in InsertedItems)
                        {
                            var item = Activator.CreateInstance(LookUpInfo.ItemType) as BusinessObject;
                            if (item != null)
                            {
                                item.SetProperty(ValueMember, p);
                                item.SetProperty(DisplayMember, p);
                                itemsSource.Insert(0, item);
                            }
                        }
                    }
                }

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
            var editor = edit as ConfiguratorComboBoxEdit;
            if (editor != null)
            {
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

        ~ConfiguratorComboBoxEditSettings()
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
}
