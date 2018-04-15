using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.Utils.Serializing;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public partial class CustomGridControl : GridControl, IDisposable
    {
        #region .  Fields  .
        private readonly Version _version = new Version(1, 0, 0, 0);
        private ContentPresenter _topTotalItemRow;
        private ContentPresenter _bottomTotalItemRow;
        private ContentPresenter _updateTimeRow;
        private string _filenamepath;
        private BarButtonItem _miShowAutoFilter;
        private BarButtonItem _miShowTotalSummary;
        private ExpressionStyle _expressionStyleOptions;
        private bool _isLoaded;
        #endregion .  Fields  .

        public CustomGridControl()
        {
            DXSerializer.SetLayoutVersion(this, _version.ToString());
            ExpressionStyleOptions = new ExpressionStyle();
            InitializeComponent();
            AddHandler(CopyingToClipboardEvent, (CopyingToClipboardEventHandler)HandleCopyingToClipboard);
        }

        #region .  Properties  .

        /// <summary>
        /// Флаг, позволяющий указать отображать ли автофильтр
        /// </summary>
        public bool IsAutoFilterVisible
        {
            get
            {
                var tableView = View as TableView;
                if (tableView == null)
                    return false;

                return tableView.ShowAutoFilterRow;
            }
            set
            {
                var tableView = View as TableView;
                if (tableView == null)
                    throw new DeveloperException("IsAutoFilterVisible can be set only for TableView");

                tableView.ShowAutoFilterRow = value;
            }
        }

        public static readonly DependencyProperty AutoShowAutoFilterRowWhenRowsCountMoreThanProperty =
            DependencyProperty.Register("AutoShowAutoFilterRowWhenRowsCountMoreThan", typeof(long), typeof(CustomGridControl), new PropertyMetadata((long)-1));

        /// <summary>
        /// Кол-во строк, привысив которое при отображении, пользователю будет выведена строка автофильтра
        /// При установке отрицательного значения - данная функционально отключается
        /// </summary>
        public long AutoShowAutoFilterRowWhenRowsCountMoreThan
        {
            get { return (long)GetValue(AutoShowAutoFilterRowWhenRowsCountMoreThanProperty); }
            set { SetValue(AutoShowAutoFilterRowWhenRowsCountMoreThanProperty, value); }
        }

        /// <summary>
        /// Всего записей в гриде.
        /// </summary>
        public long RowsCount
        {
            get
            {
                var data = ItemsSource as IList;
                return data == null ? 0 : data.Count;
            }
        }

        /// <summary>
        /// Показать индикатор количества строк (Dependency property). 
        /// </summary>
        public bool ShowTotalRow
        {
            get { return (bool)GetValue(ShowTotalRowProperty); }
            set { SetValue(ShowTotalRowProperty, value); }
        }
        public static readonly DependencyProperty ShowTotalRowProperty = DependencyProperty.Register("ShowTotalRow", typeof(bool), typeof(CustomGridControl), new PropertyMetadata(OnShowTotalRowPropertyChanged));

        private static void OnShowTotalRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (CustomGridControl)d;
            source.OnTotalRowPositionPropertyChanged(source.TotalRowPosition);
        }

        /// <summary>
        /// Символ индикации того, что не все строки из БД получены (Dependency property).
        /// </summary>
        public object TotalRowItemFilteredSymbol
        {
            get { return GetValue(TotalRowItemFilteredSymbolProperty); }
            set { SetValue(TotalRowItemFilteredSymbolProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemFilteredSymbolProperty = DependencyProperty.Register("TotalRowItemFilteredSymbol", typeof(object), typeof(CustomGridControl));

        /// <summary>
        /// Формат индикатора количества строк. Формат 'Всего строк' - {0}, 'Формат видимых строк' - {1}, 'Символ - не все строки из БД получены' - {2}, 'Дополнительная информация' - {3} (Dependency property).
        /// </summary>
        public string TotalRowItemDisplayFormat
        {
            get { return (string)GetValue(TotalRowItemDisplayFormatProperty); }
            set { SetValue(TotalRowItemDisplayFormatProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemDisplayFormatProperty = DependencyProperty.Register("TotalRowItemDisplayFormat", typeof(string), typeof(CustomGridControl));

        /// <summary>
        /// Формат видимых строк (Dependency property).
        /// </summary>
        public string TotalRowItemVisibleRowDisplayFormat
        {
            get { return (string)GetValue(TotalRowItemVisibleRowDisplayFormatProperty); }
            set { SetValue(TotalRowItemVisibleRowDisplayFormatProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemVisibleRowDisplayFormatProperty = DependencyProperty.Register("TotalRowItemVisibleRowDisplayFormat", typeof(string), typeof(CustomGridControl));

        public bool TotalRowItemFilteredSymbolIsVisible
        {
            get { return (bool)GetValue(TotalRowItemFilteredSymbolIsVisibleProperty); }
            set { SetValue(TotalRowItemFilteredSymbolIsVisibleProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemFilteredSymbolIsVisibleProperty = DependencyProperty.Register("TotalRowItemFilteredSymbolIsVisible", typeof(bool), typeof(CustomGridControl), new PropertyMetadata(OnTotalRowItemFilteredSymbolIsVisiblePropertyChanged));

        private static void OnTotalRowItemFilteredSymbolIsVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (CustomGridControl)d;
            source.TotalRowItemDataContextUpdate(source.TotalRowItemContentUpdate());
        }

        public TotalRowPositionType TotalRowPosition
        {
            get { return (TotalRowPositionType)GetValue(TotalRowPositionProperty); }
            set { SetValue(TotalRowPositionProperty, value); }
        }
        public static readonly DependencyProperty TotalRowPositionProperty = DependencyProperty.Register("TotalRowPosition", typeof(TotalRowPositionType), typeof(CustomGridControl), new PropertyMetadata(TotalRowPositionType.Top, OnTotalRowPositionPropertyChanged));

        private static void OnTotalRowPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomGridControl)d).OnTotalRowPositionPropertyChanged((TotalRowPositionType)e.NewValue);
        }

        private void OnTotalRowPositionPropertyChanged(TotalRowPositionType newvalue)
        {
            TotalRowItemDataContextUpdate(TotalRowItemContentUpdate());
            if (!IsLoaded)
                return;
            if (!ShowTotalRow)
            {
                VisualStateManager.GoToState(this, "TotalItemRowHidden", true);
                return;
            }
            switch (newvalue)
            {
                case TotalRowPositionType.Bottom:
                    VisualStateManager.GoToState(this, "BottomTotalItemRowVisible", true);
                    break;
                case TotalRowPositionType.Top:
                    VisualStateManager.GoToState(this, "TopTotalItemRowVisible", true);
                    break;
            }
        }

        /// <summary>
        /// Дополнительная информация (Dependency property).
        /// </summary>
        public string TotalRowItemAdditionalInfo
        {
            get { return (string)GetValue(TotalRowItemAdditionalInfoProperty); }
            set { SetValue(TotalRowItemAdditionalInfoProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemAdditionalInfoProperty = DependencyProperty.Register("TotalRowItemAdditionalInfo", typeof(string), typeof(CustomGridControl), new PropertyMetadata(OnTotalRowItemAdditionalInfoPropertyChanged));

        private static void OnTotalRowItemAdditionalInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (CustomGridControl)d;
            source.TotalRowItemDataContextUpdate(source.TotalRowItemContentUpdate());
        }

        /// <summary>
        /// Необходимость передернуть время обновления (Dependency property).
        /// </summary>
        public bool IsNeedRefresh
        {
            get { return (bool)GetValue(IsNeedRefreshProperty); }
            set { SetValue(IsNeedRefreshProperty, value); }
        }
        public static readonly DependencyProperty IsNeedRefreshProperty = DependencyProperty.Register("IsNeedRefresh", typeof(bool), typeof(CustomGridControl), new PropertyMetadata(OnIsNeedRefreshPropertyChanged));

        private static void OnIsNeedRefreshPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomGridControl)d).IsNeedRefresh = (bool)e.NewValue;
        }

        /// <summary>
        /// Коллекция стилей по условию.
        /// </summary>
        [DevExpress.Utils.Serializing.XtraSerializableProperty(XtraSerializationVisibility.Hidden)]
        public ExpressionStyle ExpressionStyleOptions
        {
            get { return _expressionStyleOptions; }
            private set
            {
                _expressionStyleOptions = value;
                OnExpressionStyleOptionsChanged();
            }
        }

        public event EventHandler ExpressionStyleOptionsChanged;
        public event EventHandler RestoredLayoutFromXml;

        protected override Type ColumnType
        {
            get { return typeof(CustomGridColumn); }
        }

        /// <summary>
        /// Если значение свойства True, то не показываем кастомные меню.
        /// </summary>
        public bool UseStandardGridMenu { get; set; }

        #endregion .  Properties  .

        #region .  Methods  .
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _topTotalItemRow = GetTemplateChild("topTotalItemRow") as ContentPresenter;
            _bottomTotalItemRow = GetTemplateChild("bottomTotalItemRow") as ContentPresenter;
            _updateTimeRow = GetTemplateChild("updateTimeRow") as ContentPresenter;

            //DevExpress clears DataContext.
            if (_topTotalItemRow != null)
            {
                _topTotalItemRow.DataContextChanged -= OnTotalItemRowOnDataContextChanged;
                _topTotalItemRow.DataContextChanged += OnTotalItemRowOnDataContextChanged;
            }
            if (_topTotalItemRow != null)
            {
                _topTotalItemRow.DataContextChanged -= OnTotalItemRowOnDataContextChanged;
                _topTotalItemRow.DataContextChanged += OnTotalItemRowOnDataContextChanged;
            }

            PropertyChanged -= OnPropertyChanged;
            PropertyChanged += OnPropertyChanged;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);

            if (_isLoaded)
                return;
            _isLoaded = true;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            OnTotalRowPositionPropertyChanged(TotalRowPosition);
            if (_filenamepath != null)
            {
                RestoreLayoutFromXml(_filenamepath);
                _filenamepath = null;
                OnRestoredLayoutFromXml();
            }

            RefreshAutoFilterVisible();

            //Альтернативные строки
            var tableView = View as TableView;
            if (tableView == null)
                return;

            if (Application.Current == null)
                return;

            var themeName = ThemeManager.ActualApplicationThemeName;
            if (Application.Current.Resources.Contains(themeName))
            {
                tableView.AlternateRowBackground = Application.Current.Resources[themeName] as Brush;
                tableView.AlternationCount = 2;
                return;
            }

            if (!Application.Current.Resources.Contains(StyleKeys.EvenRowBrushKey))
                return;

            tableView.AlternateRowBackground = Application.Current.Resources[StyleKeys.EvenRowBrushKey] as Brush;
            tableView.AlternationCount = 2;
        }

        private void RefreshColumnFilterMode()
        {
            foreach (var column in Columns)
            {
                // для Lookup-ов выставляем режим поиска по Value
                if (column.EditSettings is CustomCommonLookupSettings || column.EditSettings is CheckEditSettings)
                    column.ColumnFilterMode = ColumnFilterMode.Value;
            }
        }

        protected override void OnViewChanged(DataViewBase oldValue, DataViewBase newValue)
        {
            base.OnViewChanged(oldValue, newValue);

            if (oldValue != null)
            {
                oldValue.ShowGridMenu -= OnShowGridMenu;
                if (_miShowAutoFilter != null)
                    _miShowAutoFilter.ItemClick -= ShowAutoFilterOnItemClick;
                if (_miShowTotalSummary != null)
                    _miShowTotalSummary.ItemClick -= ShowTotalSummaryOnItemClick;

                _miShowAutoFilter = null;

                var oldTableView = oldValue as TableView;
                if (oldTableView != null)
                {
                    oldTableView.RowDoubleClick -= TableViewOnRowDoubleClick;
                    ItemsSourceChanged -= OnItemsSourceChanged;
                }
            }

            var tableView = newValue as TableView;
            if (tableView != null)
            {
                tableView.ShowGridMenu += OnShowGridMenu;

                ApplyPrefomanceSettings();

                // задаем специальный стиль для отображения автофильтра
                var newStyle = new Style(typeof(FilterCellContentPresenter), tableView.AutoFilterRowCellStyle);
                newStyle.Setters.Add(new Setter(BackgroundProperty, Brushes.Yellow));
                newStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
                tableView.AutoFilterRowCellStyle = newStyle;
                //tableView.AutoFilterRowCellStyle.Setters.Add(new Setter(BackgroundProperty, Colors.Yellow));

                if (!UseStandardGridMenu)
                {
                    tableView.ColumnMenuCustomizations.Add(new BarItemSeparator());

                    // добавляем кнопку для отображения автофильтра
                    _miShowAutoFilter = new BarButtonItem();
                    _miShowAutoFilter.Name = "showAutoFilterItem";
                    _miShowAutoFilter.ItemClick += ShowAutoFilterOnItemClick;
                    tableView.ColumnMenuCustomizations.Add(_miShowAutoFilter);

                    // добавляем кнопку для отображения панели суммирования
                    _miShowTotalSummary = new BarButtonItem();
                    _miShowTotalSummary.Name = "showTotalSummaryItem";
                    _miShowTotalSummary.ItemClick += ShowTotalSummaryOnItemClick;
                    tableView.ColumnMenuCustomizations.Add(_miShowTotalSummary);
                }

                tableView.RowDoubleClick += TableViewOnRowDoubleClick;
                ItemsSourceChanged += OnItemsSourceChanged;
            }
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    RemoveAutomationClients();
        //    return base.MeasureOverride(availableSize);
        //}

        private void ApplyPrefomanceSettings()
        {
            var tableView = View as TableView;
            if (tableView == null)
                return;

            tableView.ItemsSourceErrorInfoShowMode = ItemsSourceErrorInfoShowMode.None;
            tableView.AllowMasterDetail = false;
            tableView.AutoWidth = false;
            tableView.ShowGroupPanel = false;
            tableView.AllowPerPixelScrolling = true;
            tableView.ScrollAnimationMode = ScrollAnimationMode.Linear;
            tableView.AllowCascadeUpdate = true;

            // это не сильно помогает
            //tableView.RowAnimationKind = RowAnimationKind.None;
        }

        protected void OnExpressionStyleOptionsChanged()
        {
            var h = ExpressionStyleOptionsChanged;
            if (h != null)
                h(this, EventArgs.Empty);
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs itemsSourceChangedEventArgs)
        {
            RefreshAutoFilterVisible();
        }

        private void ShowTotalSummaryOnItemClick(object sender, ItemClickEventArgs e)
        {
            var tableView = (TableView)View;
            tableView.ShowTotalSummary = !tableView.ShowTotalSummary;
        }

        private void ShowAutoFilterOnItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var tableView = (TableView)View;
            // переключаем строчки
            tableView.ShowAutoFilterRow = !tableView.ShowAutoFilterRow;
        }

        private void TableViewOnRowDoubleClick(object sender, RowDoubleClickEventArgs e)
        {
            var tableView = sender as TableView;
            if (tableView == null)
                return;

            // отключаем командны для Autofilter
            if (e.HitInfo.RowHandle == AutoFilterRowHandle)
                e.Handled = true;
        }

        private void OnShowGridMenu(object sender, GridMenuEventArgs e)
        {
            if (_miShowAutoFilter == null || UseStandardGridMenu)
                return;

            var tableView = sender as TableView;
            if (tableView == null)
                return;

            // отключаем контекстное меню для Autofilter
            if (e.MenuType == GridMenuType.RowCell)
            {
                var hi = tableView.CalcHitInfo((DependencyObject)e.TargetElement);
                if (hi.RowHandle == AutoFilterRowHandle)
                    e.Handled = true;
            }

            _miShowAutoFilter.Content = tableView.ShowAutoFilterRow
                ? Properties.Resources.HideAutoFilterMenuItemContent
                : Properties.Resources.ShowAutoFilterMenuItemContent;

            _miShowTotalSummary.Content = tableView.ShowTotalSummary
                ? Properties.Resources.HideTotalSummaryContent
                : Properties.Resources.ShowTotalSummaryContent;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "VisibleRowCount")
                return;

            TotalRowItemDataContextUpdate(TotalRowItemContentUpdate());
            TotalRowItemDataContextUpdate(_updateTimeRow, DateTime.Now);
        }

        private void RefreshAutoFilterVisible()
        {
            // если включен режим автоматического появления автофильтра при большом кол-ве строк, то проверяем кол-во и отображаем
            if (AutoShowAutoFilterRowWhenRowsCountMoreThan >= 0 && VisibleRowCount > 0 && AutoShowAutoFilterRowWhenRowsCountMoreThan < VisibleRowCount)
                IsAutoFilterVisible = true;
        }

        private void OnTotalItemRowOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var control = sender as FrameworkElement;
            if (control == null)
                return;
            if (control.DataContext == null)
            {
                TotalRowItemDataContextUpdate(control, TotalRowItemContentUpdate());
            }
        }

        private void TotalRowItemDataContextUpdate(FrameworkElement control, object data)
        {
            if (control != null)
                control.DataContext = data;
        }

        private void TotalRowItemDataContextUpdate(object data)
        {
            TotalRowItemDataContextUpdate(_topTotalItemRow, data);
            TotalRowItemDataContextUpdate(_bottomTotalItemRow, data);
        }

        private string TotalRowItemContentUpdate()
        {
            if (string.IsNullOrEmpty(TotalRowItemDisplayFormat))
                return null;

            return string.Format(TotalRowItemDisplayFormat,
                RowsCount,
                VisibleRowCount == RowsCount ? null : (string.IsNullOrEmpty(TotalRowItemVisibleRowDisplayFormat) ? null : string.Format(TotalRowItemVisibleRowDisplayFormat, VisibleRowCount)),
                TotalRowItemFilteredSymbolIsVisible ? TotalRowItemFilteredSymbol : null,
                TotalRowItemAdditionalInfo);
        }

        public void RestoreLayoutFromXml(string path, int index)
        {
            if (index == 0)
            {
                if (!IsLoaded)
                {
                    _filenamepath = path;
                    return;
                }
                RestoreLayoutFromXml(path);
                OnRestoredLayoutFromXml();

                // игнорируем сохраненные настройки
                ApplyPrefomanceSettings();

                // после загрузки настроек нужно убедиться, что для Lookup-ов будет выставлен режим поиска по Value
                RefreshColumnFilterMode();
            }
            else
            {
                RestoreExpressionStyleOptionFromXml(path);
            }
        }

        protected void OnRestoredLayoutFromXml()
        {
            var handler = RestoredLayoutFromXml;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public void SaveLayoutToXml(string[] path)
        {
            if (!string.IsNullOrEmpty(path[0]))
                SaveLayoutToXml(path[0]);
            if (path.Length > 1 && !string.IsNullOrEmpty(path[1]))
                SaveExpressionStyleOptionToXml(path[1]);
        }

        private Type GetExpressionStyleOptionsType()
        {
            return typeof(ExpressionStyle);
        }

        private void RestoreExpressionStyleOptionFromXml(string path)
        {
            if (!File.Exists(path))
                return;

            var xsl = new XmlSerializer(GetExpressionStyleOptionsType());
            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var reader = XmlReader.Create(fs))
                {
                    ExpressionStyleOptions = (ExpressionStyle)xsl.Deserialize(reader);
                }
            }
        }

        private void SaveExpressionStyleOptionToXml(string path)
        {
            var xsl = new XmlSerializer(GetExpressionStyleOptionsType());
            using (var writeFileStream = new StreamWriter(path))
            {
                xsl.Serialize(writeFileStream, ExpressionStyleOptions);
                writeFileStream.Close();
            }
        }

        /// <summary>
        /// Search through the collection of rows. 
        /// The method returns the index of the row where the key is equal to Row.Key
        /// If the string is not found, returns -1
        /// </summary>
        /// <param name="key"></param>
        /// <returns>index of the row</returns>
        public int IndexOf(object key)
        {
            for (long i = 0; i < RowsCount; i++)
            {
                var obj = GetRow((int)i) as IKeyHandler;
                if (obj != null && key != null && key.Equals(obj.GetKey()))
                    return (int)i;
            }
            return -1;
        }

        public void SetFocusRow(int index)
        {
            if (index == -1)
                return;
            SelectItem(index);
            var tableView = View as TableView;
            if (tableView != null)
                tableView.FocusedRowHandle = index;
        }

        private void HandleCopyingToClipboard(object sender, CopyingToClipboardEventArgs e)
        {
            ClipboardCopyMode = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                ? ClipboardCopyMode.IncludeHeader
                : ClipboardCopyMode.ExcludeHeader;
        }

        //private static MethodInfo mi = Assembly.GetAssembly(typeof(ContentElement)).GetType("System.Windows.ContextLayoutManager").GetMethod("From", BindingFlags.Static | BindingFlags.NonPublic);
        //private static PropertyInfo _aepi;
        //private static FieldInfo _cfi;
        //private static FieldInfo _hfi;
        //private static MethodInfo _rimi;

        //private void RemoveAutomationClients()
        //{
        //    object clm = mi.Invoke(null, new object[] { Dispatcher });

        //    PropertyInfo aepi = _aepi ?? (_aepi = clm.GetType().GetProperty("AutomationEvents", BindingFlags.Instance | BindingFlags.NonPublic));
        //    object ae = aepi.GetValue(clm, null);

        //    FieldInfo cfi = _cfi ?? (_cfi = ae.GetType().GetField("_count", BindingFlags.Instance | BindingFlags.NonPublic));
        //    FieldInfo hfi = _hfi ?? (_hfi = ae.GetType().GetField("_head", BindingFlags.Instance | BindingFlags.NonPublic));
        //    MethodInfo rimi = _rimi ?? (_rimi = ae.GetType().GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic));

        //    while (((int)cfi.GetValue(ae)) > 0)
        //    {
        //        object listItem = hfi.GetValue(ae);
        //        rimi.Invoke(ae, new [] { listItem });
        //    }
        //}
        #endregion Methods

        public void Dispose()
        {
            foreach (var column in Columns)
            {
                var dispEditSetts = column.EditSettings as IDisposable;
                if (dispEditSetts != null)
                    dispEditSetts.Dispose();
            }
        }
    }

    public class CustomGridColumn : GridColumn, IDisposable
    {
        #region .  Properties  .
        public string SerializableName
        {
            get { return (string)GetValue(SerializableNameProperty); }
            set { SetValue(SerializableNameProperty, value); }
        }
        public static readonly DependencyProperty SerializableNameProperty = DependencyProperty.Register("SerializableName", typeof(string), typeof(CustomGridColumn), new PropertyMetadata(default(string), OnSerializableNameChanged));

        /// <summary>
        /// Path для создания Binding'а (Dependency property).  
        /// </summary>
        public string BindingPath
        {
            get { return (string)GetValue(BindingPathProperty); }
            set { SetValue(BindingPathProperty, value); }
        }
        public static readonly DependencyProperty BindingPathProperty = DependencyProperty.Register("BindingPath", typeof(string), typeof(CustomGridColumn), new PropertyMetadata(OnBindingPathPropertyChanged));
        #endregion .  Properties  .

        #region .  Methods  .
        private static void OnSerializableNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomGridColumn)d).OnSerializableNameChanged((string)e.NewValue);
        }

        private static void OnBindingPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomGridColumn)d).OnBindingPathPropertyChanged(e.NewValue.To<string>());
        }

        private void OnSerializableNameChanged(string newvalue)
        {
            if (!string.IsNullOrEmpty(newvalue))
                Name = newvalue.Replace('.', '_').Replace('[', '_').Replace(']', '_');
            SetProperies();
        }

        private void OnBindingPathPropertyChanged(string newvalue)
        {
            if (string.IsNullOrEmpty(newvalue))
                return;

            var binding = new Binding { Path = new PropertyPath(newvalue) };

            var field = GetSourceField();
            if (field == null)
                return;

            IValueConverter valueConverter;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.BindingIValueConverter, false, out valueConverter))
                binding.Converter = valueConverter;

            Binding = binding;
        }

        private DataField GetSourceField()
        {
            return DataContext as DataField;
        }

        private void SetProperies()
        {
            var field = GetSourceField();
            if (field == null)
                return;

            double dvalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ColumnWidth, true, out dvalue))
            {
                if (dvalue > 0)
                    Width = dvalue;
            }

            string svalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ColumnBestFitMode, true, out svalue))
                BestFitMode = svalue.To(BestFitMode.Default);
        }

        public void Dispose()
        {
            //TODO: попробовать еще и убивать Editor-ы (Lookup-ы)
            var dispSettings = EditSettings as IDisposable;
            if (dispSettings != null)
                dispSettings.Dispose();
        }
        #endregion
    }

    public enum TotalRowPositionType
    {
        Bottom,
        Top
    }
}
