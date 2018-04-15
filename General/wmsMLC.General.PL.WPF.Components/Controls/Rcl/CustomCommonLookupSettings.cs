using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    /// <summary>
    /// Класс для задач настройки Lookup-ов (Combobox, LookUp).
    /// </summary>
    internal class CustomCommonLookupSettings : ComboBoxEditSettings, IDisposable
    {
        public static readonly DependencyProperty LookUpCodeEditorProperty = DependencyProperty.Register("LookUpCodeEditor", typeof(string), typeof(CustomCommonLookupSettings), new PropertyMetadata(OnLookUpCodeEditorPropertyChanged));
        public static readonly DependencyProperty AllowAddNewValueProperty = DependencyProperty.Register("AllowAddNewValue", typeof(bool), typeof(CustomCommonLookupSettings));

        private string _filter0;
        private IBaseManager _managerInstance;

        static CustomCommonLookupSettings()
        {
            EditorSettingsProvider.Default.RegisterUserEditor(typeof(CustomComboBoxEdit), typeof(CustomCommonLookupSettings),
                () => new CustomComboBoxEdit(),
                () => new CustomCommonLookupSettings());
        }
        public CustomCommonLookupSettings()
        {
            AllowAddNewValue = true;
        }

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~CustomCommonLookupSettings()
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
            if (_managerInstance != null)
            {
                _managerInstance.Changed -= ManagerInstanceOnChanged;
                _managerInstance.Dispose();
                _managerInstance = null;
            }
        }
        #endregion

        #region .  Properties  .
        public string LookUpCodeEditor
        {
            get { return (string)GetValue(LookUpCodeEditorProperty); }
            set { SetValue(LookUpCodeEditorProperty, value); }
        }

        private LookupInfo LookUpInfo { get; set; }

        /// <summary>
        /// Разрешаем добавлять новую запись в ItemsSource, если для данного значения не найден лукап. Это Dependency Property.
        /// </summary>
        public bool AllowAddNewValue
        {
            get { return (bool)GetValue(AllowAddNewValueProperty); }
            set { SetValue(AllowAddNewValueProperty, value); }
        }

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

        private GridColumn ParentColumn
        {
            get { return Parent as GridColumn; }
        }

        private bool? IsWfDesignMode
        {
            get
            {
                var column = ParentColumn;
                if (column == null)
                    return null;

                var grid = column.Parent as RclGridControl;
                if (grid == null)
                    return null;

                return grid.IsWfDesignMode;
            }
        }
        #endregion .  Properties  .

        private static void OnLookUpCodeEditorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomCommonLookupSettings)d).OnLookUpCodeEditorPropertyChanged();
        }

        private void OnLookUpCodeEditorPropertyChanged()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                return;

            var isWfDesignMode = IsWfDesignMode;
            if (!isWfDesignMode.HasValue || isWfDesignMode == false)
            {
                var column = ParentColumn;
                if (column != null && (!column.Visible || column.Parent == null))
                    return;
            }

            LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
            DisplayMember = LookUpInfo.DisplayMember;
            ValueMember = LookUpInfo.ValueMember;

            ManagerInstance = LookupHelper.GetItemSourceManager(LookUpInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;
            LookupHelper.InitializeVarFilter(LookUpInfo.Filter, out _filter0);
            RefreshData();
        }

        public Type ItemType
        {
            get
            {
                return LookUpInfo == null ? null : LookupHelper.GetItemSourceType(LookUpInfo);
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

        private async void RefreshData()
        {
            try
            {
                IsInLoading = true;
                var items = await GetDataAsync();
                Dispatcher.Invoke((Action)(() =>
                {
                    ItemsSource = items;
                    //new PropertyCoercionHelper((BaseEdit) Editor).Update();
                }));

            }
            finally
            {
                IsInLoading = false;
            }
        }

        private async Task<IEnumerable<object>> GetDataAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                //using (var mgr = LookupHelper.GetItemSourceManager(LookUp))
                return ManagerInstance.GetFiltered(_filter0, GetModeEnum.Partial);
            });
        }

        protected override void AssignToEditCore(IBaseEdit edit)
        {
            var editor = edit as CustomComboBoxEdit;
            if (editor != null)
                editor.IsSettings = true;
            base.AssignToEditCore(edit);
        }
    }
}
