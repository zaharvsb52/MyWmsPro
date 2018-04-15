using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Editors.Settings;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Views.Controls
{
    /// <summary>
    /// Базовый класс для Settings-ов всех Lookup-подобных сущностей
    /// </summary>
    public abstract class CustomBaseLookupEditSetting : LookUpEditSettingsBase, IDisposable
    {
        #region .  Fields  .
        public static readonly DependencyProperty LookUpCodeEditorProperty = DependencyProperty.Register("LookUpCodeEditor", typeof(string), typeof(CustomBaseLookupEditSetting), new PropertyMetadata(OnLookUpCodeEditorPropertyChanged));
        public static readonly DependencyProperty AllowAddNewValueProperty = DependencyProperty.Register("AllowAddNewValue", typeof(bool), typeof(CustomBaseLookupEditSetting));

        private string _filter0;
        private IBaseManager _managerInstance;
        #endregion .  Fields  .

        protected CustomBaseLookupEditSetting()
        {
            Loaded += OnLoaded;
        }

        #region .  Properties  .
        public string LookUpCodeEditor
        {
            get { return (string)GetValue(LookUpCodeEditorProperty); }
            set { SetValue(LookUpCodeEditorProperty, value); }
        }

        /// <summary>
        /// Разрешаем добавлять новую запись в ItemsSource, если для данного значения не найден лукап. Это Dependency Property.
        /// </summary>
        public bool AllowAddNewValue
        {
            get { return (bool)GetValue(AllowAddNewValueProperty); }
            set { SetValue(AllowAddNewValueProperty, value); }
        }

        protected LookupInfo LookUpInfo { get; set; }

        protected IBaseManager ManagerInstance
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

        public bool NotLoadDataFromEditor { get; set; }
        #endregion .  Properties  .

        #region .  Finalize & Dispose  .
        /// <summary> Признак того, что освобождение ресурсов уже произошло </summary>
        private bool _disposed;

        ~CustomBaseLookupEditSetting()
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

        #region .  Methods  .
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //Загружаем данные, если работаем в режиме редактирования грида
            Loaded -= OnLoaded;
            if (NotLoadDataFromEditor)
                RefreshData();
        }

        private static void OnLookUpCodeEditorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomBaseLookupEditSetting)d).OnLookUpCodeEditorPropertyChanged();
        }

        protected virtual void OnLookUpCodeEditorPropertyChanged()
        {
            if (string.IsNullOrEmpty(LookUpCodeEditor))
                return;

            if (LookUpInfo == null)
                LookUpInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
            DisplayMember = LookUpInfo.DisplayMember;
            ValueMember = LookUpInfo.ValueMember;

//            // определяем тип данных
//            var lookupSourceItemType = LookupHelper.GetItemSourceType(LookUp);
//
//            // если извне хотят управлять колонками - пусть сам и управляют
//            if (LookUpColumnsSource == null)
//                LookUpColumnsSource = DataFieldHelper.Instance.GetDataFields(lookupSourceItemType, SettingDisplay.LookUp);

            ManagerInstance = LookupHelper.GetItemSourceManager(LookUpInfo);
            ManagerInstance.AllowMonitorChangesInOtherInsances = true;
            LookupHelper.InitializeVarFilter(LookUpInfo.Filter, out _filter0);

            //TODO: Возможно данные получать стразу не нужно
            //RefreshData();
        }

        protected virtual void ManagerInstanceOnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    RefreshData();
                    break;
            }
        }

        protected async void RefreshData()
        {
            if (!NotLoadDataFromEditor && string.Equals(DisplayMember, ValueMember))
                return;

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
                var attrEntity = GetAttrEntity();
                var filter = GetFilter();
                return ManagerInstance.GetFiltered(filter, attrEntity);
            });
        }

        protected virtual string GetFilter()
        {
            return _filter0;
        }

        protected virtual string GetAttrEntity()
        {
            return XmlDocumentConverter.GetShortTemplate(LookUpInfo.ItemType).OuterXml;
        }

        #endregion .  Methods  .
    }
}