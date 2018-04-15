using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Serialization;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using System.Xml.Serialization;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public partial class CustomGridControl : GridControl, ISaveRestore, IDisposable
    {
        #region .  Fields  .
        public Brush AutoFilterRowDefaultColor = Brushes.Yellow;
        public bool ByCheckCopyMode = true;

        private readonly Version _version = new Version(1, 0, 0, 0);
        private ContentPresenter _topTotalItemRow;
        private ContentPresenter _bottomTotalItemRow;
        private ContentPresenter _updateTimeRow;
        private string _xmlFileNamePath;
        private string _layoutString;
        private BarButtonItem _miShowAutoFilter;
        private BarButtonItem _miShowTotalSummary;
        private ExpressionStyle _expressionStyleOptions;
        private bool _allowMasterDetail;
        private ItemsSourceErrorInfoShowMode _showErrorMode;
        private bool _isLoaded;

        private static readonly XmlSerializer XpressionStyleSerializer = new XmlSerializer(typeof (ExpressionStyle));
        #endregion .  Fields  .

        public CustomGridControl()
        {
            DXSerializer.SetLayoutVersion(this, _version.ToString());
            ExpressionStyleOptions = new ExpressionStyle();
            InitializeComponent();
            CopyingToClipboard += OnCopyingToClipboard;

            IsEnabledChanged += OnIsEnabledChanged;
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //fix DXBug with non visual elements properties binding
            // https://www.devexpress.com/Support/Center/Question/Details/B201223
            foreach (var column in Columns)
                column.IsEnabled = (bool)e.NewValue;
        }

        #region .  Properties  .

        /// <summary>
        /// Флаг, позволяющий указать отображать ли автофильтр.
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

        public bool AllowMasterDetail
        {
            get
            {
                return _allowMasterDetail;
            }
            set
            {
                _allowMasterDetail = value;
                ApplyAllowMasterDetail();
            }
        }

        public ItemsSourceErrorInfoShowMode ShowErrorMode
        {
            get
            {
                return _showErrorMode;
            }
            set
            {
                _showErrorMode = value;
                ApplyShowErrorMode();
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
        [DevExpress.Utils.Serializing.XtraSerializableProperty(DevExpress.Utils.Serializing.XtraSerializationVisibility.Hidden)]
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

        public static readonly RoutedEvent RowDoubleClickEvent = EventManager.RegisterRoutedEvent("RowDoubleClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomGridControl));
        public event RoutedEventHandler RowDoubleClick
        {
            add { AddHandler(RowDoubleClickEvent, value); }
            remove { RemoveHandler(RowDoubleClickEvent, value); }
        }

        #endregion .  Properties  .

        #region .  Methods  .
        //Не удалять - используется для исправления неправильных настроек вида
        //protected override void OnUpdateRowsCore()
        //{
        //    base.OnUpdateRowsCore();

        //    if (Columns != null)
        //    {
        //        foreach (var c in Columns.Where(c => (c.FieldType == typeof(DateTime?) || c.FieldType == typeof(DateTime)) && c.ColumnFilterMode != ColumnFilterMode.Value).ToArray())
        //        {
        //            c.ColumnFilterMode = ColumnFilterMode.Value;
        //        }
        //    }
        //}

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
            RestoreLayoutInternal();

            RefreshAutoFilterVisible();

            //Альтернативные строки
            var tableView = View as TableView;
            if (tableView == null)
                return;

            if (Application.Current == null)
                return;

            var themeName = ThemeManager.ActualApplicationThemeName;
            if (themeName == null)
                return;

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
                // для CheckBox-ов выставляем режим поиска по Value
                if (column.EditSettings is CheckEditSettings)
                    column.ColumnFilterMode = ColumnFilterMode.Value;
                else if (column.EditSettings is CustomGridSimpleLookupEditSettings)
                {
                    column.ColumnFilterMode = ColumnFilterMode.Value;
                }
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
                    oldTableView.RowDoubleClick -= OnViewRowDoubleClick;
                    ItemsSourceChanged -= OnItemsSourceChanged;
                }
            }

            var newTableView = newValue as TableView;
            if (newTableView != null)
            {
                newTableView.ShowGridMenu += OnShowGridMenu;
                newTableView.ShowingEditor += OnShowingEditor;
                newTableView.ShowFilterPopup += OnShowFilterPopup;

                ApplyPrefomanceSettings();

                // задаем специальный стиль для отображения автофильтра
                var newStyle = new Style(typeof(FilterCellContentPresenter), newTableView.AutoFilterRowCellStyle);
                newStyle.Setters.Add(new Setter(BackgroundProperty, AutoFilterRowDefaultColor));
                newTableView.AutoFilterRowCellStyle = newStyle;

                if (!UseStandardGridMenu)
                {
                    newTableView.ColumnMenuCustomizations.Add(new BarItemSeparator());

                    // добавляем кнопку для отображения автофильтра
                    _miShowAutoFilter = new BarButtonItem {Name = "showAutoFilterItem"};
                    _miShowAutoFilter.ItemClick += ShowAutoFilterOnItemClick;
                    newTableView.ColumnMenuCustomizations.Add(_miShowAutoFilter);

                    // добавляем кнопку для отображения панели суммирования
                    _miShowTotalSummary = new BarButtonItem {Name = "showTotalSummaryItem"};
                    _miShowTotalSummary.ItemClick += ShowTotalSummaryOnItemClick;
                    newTableView.ColumnMenuCustomizations.Add(_miShowTotalSummary);
                }

                newTableView.RowDoubleClick += OnViewRowDoubleClick;
                ItemsSourceChanged += OnItemsSourceChanged;
            }
        }

        private void OnShowFilterPopup(object sender, FilterPopupEventArgs ea)
        {
            var setts = ea.Column.EditSettings as CustomGridSimpleLookupEditSettings;
            if (setts == null)
                return;

            var items = setts.GetAutoFilterItems(ea.Column.FieldName);
            if (items == null)
                return;

            ea.ComboBoxEdit.ItemsSource = items;

            RaiseFilterChanged();
        }

        //protected override void RaiseFilterChanged()
        //{
        //    base.RaiseFilterChanged();
        //}

        private void OnShowingEditor(object sender, ShowingEditorEventArgs e)
        {
            var setts = e.Column.EditSettings as CustomGridSimpleLookupEditSettings;
            if (setts != null)
            {
                // такой вариант возможен , если DisplayMember = ValueMember
                e.Cancel = !setts.AllowEditAutofilter;
            }
        }

        private void ApplyShowErrorMode()
        {
            var tableView = View as TableView;
            if (tableView == null)
                return;

            tableView.ItemsSourceErrorInfoShowMode = ShowErrorMode;
        }

        private void ApplyAllowMasterDetail()
        {
            var tableView = View as TableView;
            if (tableView == null)
                return;

            tableView.AllowMasterDetail = AllowMasterDetail;
        }

        private void ApplyPrefomanceSettings()
        {
            var tableView = View as TableView;
            if (tableView == null)
                return;

            ApplyAllowMasterDetail();
            ApplyShowErrorMode();
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

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
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

        private void OnViewRowDoubleClick(object sender, RowDoubleClickEventArgs e)
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
                ? StringResources.HideAutoFilterMenuItemContent
                : StringResources.ShowAutoFilterMenuItemContent;

            _miShowTotalSummary.Content = tableView.ShowTotalSummary
                ? StringResources.HideTotalSummaryContent
                : StringResources.ShowTotalSummaryContent;
        }

        private void OnCopyingToClipboard(object sender, CopyingToClipboardEventArgs e)
        {
            if (!ByCheckCopyMode)
                return;

            ClipboardCopyMode = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                ? ClipboardCopyMode.IncludeHeader
                : ClipboardCopyMode.ExcludeHeader;
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
            if (AutoShowAutoFilterRowWhenRowsCountMoreThan >= 0 && AutoShowAutoFilterRowWhenRowsCountMoreThan < VisibleRowCount)
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
                    _xmlFileNamePath = path;
                    return;
                }
                RestoreLayoutFromXml(path);
                OnRestoredLayoutFromXml();

                // игнорируем сохраненные настройки
                ApplyPrefomanceSettings();

                // после загрузки настроек нужно убедиться, что для CheckBox-ов будет выставлен режим поиска по Value
                RefreshColumnFilterMode();
            }
            else
            {
                RestoreExpressionStyleOptionFromXml(path);
            }
        }

        public void RestoreLayoutFromString(string layout, int index)
        {
            if (index == 0)
            {
                if (!IsLoaded)
                {
                    _layoutString = layout;
                    return;
                }
                RestoreLayoutFromStringBase(layout);

                // игнорируем сохраненные настройки
                ApplyPrefomanceSettings();

                // после загрузки настроек нужно убедиться, что для CheckBox-ов будет выставлен режим поиска по Value
                RefreshColumnFilterMode();
            }
            else
            {
                RestoreExpressionStyleOptionFromString(layout);
            }
        }

        private void RestoreLayoutInternal()
        {
            if (_xmlFileNamePath != null)
            {
                RestoreLayoutFromXml(_xmlFileNamePath);
                _xmlFileNamePath = null;
                OnRestoredLayoutFromXml();
                return;
            }

            if (_layoutString != null)
            {
                RestoreLayoutFromStringBase(_layoutString);
                _layoutString = null;
            }
        }

        private void RestoreLayoutFromStringBase(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    sw.AutoFlush = true;
                    sw.Write(layout);
                    ms.Position = 0;
                    RestoreLayoutFromStream(ms);
                }
            }
            OnRestoredLayoutFromXml();
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

        public string[] SaveLayoutToString()
        {
            var result = new List<string>();
            using (var ms = new MemoryStream())
            {
                SaveLayoutToStream(ms);
                ms.Position = 0;
                using (var sr = new StreamReader(ms))
                {
                    result[0] = sr.ReadToEnd();
                }
            }

            result[1] = SaveExpressionStyleOptionToString();
            return result.ToArray();
        }

        private void RestoreExpressionStyleOptionFromXml(string path)
        {
            if (!File.Exists(path))
                return;

            using (var fs = new FileStream(path, FileMode.Open))
            {
                using (var reader = XmlReader.Create(fs))
                {
                    ExpressionStyleOptions = (ExpressionStyle)XpressionStyleSerializer.Deserialize(reader);
                }
            }
        }

        private void RestoreExpressionStyleOptionFromString(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            using (var sr = new StringReader(layout))
            {
                using (var wr = XmlReader.Create(sr))
                {
                    ExpressionStyleOptions = (ExpressionStyle) XpressionStyleSerializer.Deserialize(wr);
                }
            }
        }

        private void SaveExpressionStyleOptionToXml(string path)
        {
            ExpressionStyleOptions.Version = ExpressionStyle.ActualVersion.ToString();
            using (var writeFileStream = new StreamWriter(path))
            {
                XpressionStyleSerializer.Serialize(writeFileStream, ExpressionStyleOptions);
                writeFileStream.Close();
            }
        }

        private string SaveExpressionStyleOptionToString()
        {
            ExpressionStyleOptions.Version = ExpressionStyle.ActualVersion.ToString();
             var sb = new StringBuilder();
            using (var wr = XmlWriter.Create(sb))
            {
                XpressionStyleSerializer.Serialize(wr, ExpressionStyleOptions);
            }
            return sb.ToString();
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDoubleClick(e);
            if (e.Handled)
                return;

            e.Handled = RaiseRowDoubleClickEvent(e);
        }

        private bool RaiseRowDoubleClickEvent(MouseButtonEventArgs e)
        {
            var rowHandle = View.GetRowHandleByMouseEventArgs(e);
            if (rowHandle == InvalidRowHandle || rowHandle == AutoFilterRowHandle)
                return e.Handled;

            var newEventArgs = new RoutedEventArgs(RowDoubleClickEvent);
            RaiseEvent(newEventArgs);
            return newEventArgs.Handled;
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
        public static readonly DependencyProperty SerializableNameProperty = DependencyProperty.Register("SerializableName", typeof(string), typeof(CustomGridColumn), new PropertyMetadata(default(string), OnSerializableNameChanged));
        public static readonly DependencyProperty BindingPathProperty = DependencyProperty.Register("BindingPath", typeof(string), typeof(CustomGridColumn), new PropertyMetadata(OnBindingPathPropertyChanged));

        #region .  Properties  .
        public string SerializableName
        {
            get { return (string)GetValue(SerializableNameProperty); }
            set { SetValue(SerializableNameProperty, value); }
        }

        /// <summary>
        /// Path для создания Binding'а (Dependency property).  
        /// </summary>
        public string BindingPath
        {
            get { return (string)GetValue(BindingPathProperty); }
            set { SetValue(BindingPathProperty, value); }
        }
        #endregion .  Properties  .

        #region .  Methods  .

        static void OnSerializableNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomGridColumn)d).OnSerializableNameChanged((string)e.NewValue);
        }

        static void OnBindingPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

            Binding = new Binding { Path = new PropertyPath(newvalue) };
        }

        private void SetProperies()
        {
            var field = DataContext as DataField;
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

        protected override void OnDataPropertyChanged()
        {
            base.OnDataPropertyChanged();

            var context = DataContext as DataField;
            if (context == null)
                return;

            var te = ActualEditSettings as CustomTextEditSettings;
            if (te == null)
                return;

            IValueConverter valueConverter;
            if (DataField.TryGetFieldProperties(context, ValueDataFieldConstants.BindingIValueConverter, false, out valueConverter))
                te.Converter = valueConverter;
        }

        #endregion
    }

    public enum TotalRowPositionType
    {
        Bottom,
        Top
    }

    //public class CustomTableView : TableView
    //{
    //    public void AddCustomFormatCondition(FormatConditionBase formatCondition)
    //    {
    //        if (formatCondition == null)
    //            return;

    //       var dataControl = GetDataControlModelItem();
    //        if (dataControl == null)
    //            return;

    //        var item = dataControl.Context.CreateItem(typeof (FormatCondition));
    //        if (item == null)
    //            return;

    //        var formatConditionsModelItemCollection = GetFormatConditionsModelItemCollection(dataControl);
    //        if (formatConditionsModelItemCollection == null)
    //            return;

    //        if (!string.IsNullOrEmpty(formatCondition.Expression))
    //            item.Properties[FormatConditionBase.ExpressionProperty.Name].SetValue(formatCondition.Expression);

    //        if (!string.IsNullOrEmpty(formatCondition.FieldName))
    //            item.Properties[FormatConditionBase.FieldNameProperty.Name].SetValue(formatCondition.FieldName);

    //        if (!string.IsNullOrEmpty(formatCondition.PredefinedFormatName))
    //            item.Properties[FormatConditionBase.PredefinedFormatNameProperty.Name].SetValue(
    //                formatCondition.PredefinedFormatName);

    //        var fc = formatCondition as FormatCondition;
    //        if (fc != null)
    //        {
    //            if (fc.Format != null)
    //                item.Properties[FormatCondition.FormatProperty.Name].SetValue(fc.Format);
    //        }

    //        formatConditionsModelItemCollection.Add(item);
    //    }

    //    private IModelItem GetDataControlModelItem()
    //    {
    //        return DesignTimeAdorner.GetDataControlModelItem(DataControl);
    //    }

    //    private IModelItemCollection GetFormatConditionsModelItemCollection(IModelItem dataControl)
    //    {
    //        return dataControl.Properties[GridControl.ViewProperty.GetName()].Value.Properties["FormatConditions"].Collection;
    //    }
    //}
}
