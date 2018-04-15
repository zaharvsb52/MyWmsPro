using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Settings;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomComboBoxSettings : ComboBoxEditSettings, IDisposable
    {
        #region .  Fields  .
        private string _filter0;
        private IBaseManager _managerInstance;
        #endregion .  Fields  .

        #region .  Ctors  .
        static CustomComboBoxSettings()
        {
            //EditorSettingsProvider.Default.RegisterUserEditor(typeof(CustomComboBoxEdit), typeof(CustomComboBoxSettings),
            //    () => new CustomComboBoxEdit(),
            //    () => new CustomComboBoxSettings());

            EditorSettingsProvider.Default.RegisterUserEditor2(typeof(CustomComboBoxEdit),
               typeof(CustomComboBoxSettings),
               optimized => optimized ? (IBaseEdit)new InplaceBaseEdit() : new CustomComboBoxEdit(),
               () => new CustomComboBoxSettings());
        }
        public CustomComboBoxSettings()
        {
            AllowAddNewValue = true;
            Loaded += OnLoaded;
        }
        #endregion .  Ctors  .

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~CustomComboBoxSettings()
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
            if (ManagerInstance != null)
            {
                ManagerInstance.AllowMonitorChangesInOtherInsances = false;
                ManagerInstance = null;
                LookUpInfo = null;
            }
        }
        #endregion .  Finalize & Dispose  .

        #region .  Properties  .

        public string LookUpCodeEditor
        {
            get { return (string)GetValue(LookUpCodeEditorProperty); }
            set { SetValue(LookUpCodeEditorProperty, value); }
        }
        public static readonly DependencyProperty LookUpCodeEditorProperty = DependencyProperty.Register("LookUpCodeEditor", typeof(string), typeof(CustomComboBoxSettings), new PropertyMetadata(OnLookUpCodeEditorPropertyChanged));

        /// <summary>
        /// Разрешаем добавлять новую запись в ItemsSource, если для данного значения не найден лукап. Это Dependency Property.
        /// </summary>
        public bool AllowAddNewValue
        {
            get { return (bool)GetValue(AllowAddNewValueProperty); }
            set { SetValue(AllowAddNewValueProperty, value); }
        }
        public static readonly DependencyProperty AllowAddNewValueProperty = DependencyProperty.Register("AllowAddNewValue", typeof(bool), typeof(CustomComboBoxSettings));

        public string LookUpCodeEditorFilterExt
        {
            get { return (string)GetValue(LookUpCodeEditorFilterExtProperty); }
            set { SetValue(LookUpCodeEditorFilterExtProperty, value); }
        }
        public static readonly DependencyProperty LookUpCodeEditorFilterExtProperty = DependencyProperty.Register("LookUpCodeEditorFilterExt", typeof(string), typeof(CustomComboBoxSettings), new PropertyMetadata(OnLookUpCodeEditorFilterExtPropertyChanged));

        private LookupInfo LookUpInfo { get; set; }

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

        public Type ItemType
        {
            get
            {
                return LookUpInfo == null ? null : LookupHelper.GetItemSourceType(LookUpInfo);
            }
        }

        #endregion .  Properties  .

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            RefreshData();
        }

        private static void OnLookUpCodeEditorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomComboBoxSettings)d).OnLookUpCodeEditorPropertyChanged();
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
            LookupHelper.InitializeVarFilter(LookUpInfo.Filter, out _filter0);
        }

        private static void OnLookUpCodeEditorFilterExtPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomComboBoxSettings)d).OnLookUpCodeEditorFilterExtPropertyChanged();
        }

        private void OnLookUpCodeEditorFilterExtPropertyChanged()
        {
            var filter0 = _filter0.GetTrim();
            var filter = string.IsNullOrEmpty(LookUpCodeEditorFilterExt)
                ? filter0
                : string.Format("{0}{1}{2}", filter0, string.IsNullOrEmpty(filter0) ? null : " AND ", LookUpCodeEditorFilterExt);

            _filter0 = filter;
        }

        private void ManagerInstanceOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
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
                DispatcherHelper.Invoke((Action)(() =>
                {
                    IsInLoading = false;
                    ItemsSource = items;
                }));

            }
            finally
            {
                IsInLoading = false;
            }
        }

        private async Task<IEnumerable<object>> GetDataAsync()
        {
            if (LookUpInfo == null)
                return null;

            return await Task.Factory.StartNew(() =>
                {
                    var attrEntity = LookupHelper.GetAttrEntity(LookUpInfo);
                    return ManagerInstance.GetFiltered(_filter0, attrEntity);
                });
        }

        protected override void AssignToEditCore(IBaseEdit edit)
        {
            var editor = edit as CustomComboBoxEdit;
            if (editor != null)
            {
                editor.IsSettings = true;
            }
            base.AssignToEditCore(edit);
        }

        public override string GetDisplayText(object editValue, bool applyFormatting)
        {
            var res = base.GetDisplayText(editValue, applyFormatting);
            if (editValue != null && string.IsNullOrEmpty(res))
                return editValue.ToString();
            return res;
        }

        public override string GetDisplayTextFromEditor(object editValue)
        {
            // ищем в подгруженном списке
            var item = GetDisplayValueByEditValue(editValue);

            // если нет - пытаемся получить правильный текст для отображения
            if (editValue != null && item == null)
                return base.GetDisplayTextFromEditor(editValue);

            // если есть - возвращаем
            return item == null ? string.Empty : item.ToString();
        }

        public object GetDisplayValueByEditValue(object editValue)
        {
            return ItemsProvider.GetDisplayValueByEditValue(editValue, null);
        }
    }
}
