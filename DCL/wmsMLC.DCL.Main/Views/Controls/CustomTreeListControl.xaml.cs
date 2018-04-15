using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid.TreeList;
using DevExpress.XtraGrid;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public partial class CustomTreeListControl : TreeListControl, ISaveRestore, IDisposable
    {
        #region Fields
        public Brush AutoFilterRowDefaultColor = Brushes.Yellow;
        public bool ByCheckCopyMode = true;
        private ContentPresenter _topTotalItemRow;
        private ContentPresenter _bottomTotalItemRow;
        private ContentPresenter _updateTimeRow;
        private string _xmlFileNamePath;
        private string _layoutString;
        private ExpressionStyle _expressionStyleOptions;
        private BarButtonItem _miShowAutoFilter;
        private BarButtonItem _miShowTotalSummary;
        private ItemsSourceErrorInfoShowMode _showErrorMode;
        private bool _isLoaded;
        #endregion Fields

        public CustomTreeListControl()
        {
            InitializeComponent();
            ExpressionStyleOptions = new ExpressionStyle();
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

        #region Properties
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
        public static readonly DependencyProperty ShowTotalRowProperty = DependencyProperty.Register("ShowTotalRow", typeof(bool), typeof(CustomTreeListControl), new PropertyMetadata(OnShowTotalRowPropertyChanged));

        private static void OnShowTotalRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (CustomTreeListControl)d;
            source.OnTotalRowPositionPropertyChanged(source.TotalRowPosition);
        }

        protected override Type ColumnType
        {
            get { return typeof(CustomTreeListColumn); }
        }

        /// <summary>
        /// Символ индикации того, что не все строки из БД получены (Dependency property).
        /// </summary>
        public object TotalRowItemFilteredSymbol
        {
            get { return GetValue(TotalRowItemFilteredSymbolProperty); }
            set { SetValue(TotalRowItemFilteredSymbolProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemFilteredSymbolProperty = DependencyProperty.Register("TotalRowItemFilteredSymbol", typeof(object), typeof(CustomTreeListControl));

        /// <summary>
        /// Формат индикатора количества строк. Формат 'Всего строк' - {0}, 'Формат видимых строк' - {1}, 'Символ - не все строки из БД получены' - {2}, 'Дополнительная информация' - {3} (Dependency property).
        /// </summary>
        public string TotalRowItemDisplayFormat
        {
            get { return (string)GetValue(TotalRowItemDisplayFormatProperty); }
            set { SetValue(TotalRowItemDisplayFormatProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemDisplayFormatProperty = DependencyProperty.Register("TotalRowItemDisplayFormat", typeof(string), typeof(CustomTreeListControl));

        /// <summary>
        /// Формат видимых строк (Dependency property).
        /// </summary>
        public string TotalRowItemVisibleRowDisplayFormat
        {
            get { return (string)GetValue(TotalRowItemVisibleRowDisplayFormatProperty); }
            set { SetValue(TotalRowItemVisibleRowDisplayFormatProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemVisibleRowDisplayFormatProperty = DependencyProperty.Register("TotalRowItemVisibleRowDisplayFormat", typeof(string), typeof(CustomTreeListControl));

        public bool TotalRowItemFilteredSymbolIsVisible
        {
            get { return (bool)GetValue(TotalRowItemFilteredSymbolIsVisibleProperty); }
            set { SetValue(TotalRowItemFilteredSymbolIsVisibleProperty, value); }
        }
        public static readonly DependencyProperty TotalRowItemFilteredSymbolIsVisibleProperty = DependencyProperty.Register("TotalRowItemFilteredSymbolIsVisible", typeof(bool), typeof(CustomTreeListControl), new PropertyMetadata(OnTotalRowItemFilteredSymbolIsVisiblePropertyChanged));

        private static void OnTotalRowItemFilteredSymbolIsVisiblePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (CustomTreeListControl)d;
            source.TotalRowItemDataContextUpdate(source.TotalRowItemContentUpdate());
        }

        public TotalRowPositionType TotalRowPosition
        {
            get { return (TotalRowPositionType)GetValue(TotalRowPositionProperty); }
            set { SetValue(TotalRowPositionProperty, value); }
        }
        public static readonly DependencyProperty TotalRowPositionProperty = DependencyProperty.Register("TotalRowPosition", typeof(TotalRowPositionType), typeof(CustomTreeListControl), new PropertyMetadata(TotalRowPositionType.Top, OnTotalRowPositionPropertyChanged));

        private static void OnTotalRowPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeListControl)d).OnTotalRowPositionPropertyChanged((TotalRowPositionType)e.NewValue);
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
        public static readonly DependencyProperty TotalRowItemAdditionalInfoProperty = DependencyProperty.Register("TotalRowItemAdditionalInfo", typeof(string), typeof(CustomTreeListControl), new PropertyMetadata(OnTotalRowItemAdditionalInfoPropertyChanged));

        private static void OnTotalRowItemAdditionalInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (CustomTreeListControl)d;
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
        public static readonly DependencyProperty IsNeedRefreshProperty = DependencyProperty.Register("IsNeedRefresh", typeof(bool), typeof(CustomTreeListControl), new PropertyMetadata(OnIsNeedRefreshPropertyChanged));

        private static void OnIsNeedRefreshPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeListControl)d).IsNeedRefresh = (bool)e.NewValue;
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

        /// <summary>
        /// Флаг, позволяющий указать отображать ли автофильтр.
        /// </summary>
        public bool IsAutoFilterVisible
        {
            get { return View.ShowAutoFilterRow; }
            set { View.ShowAutoFilterRow = value; }
        }

        /// <summary>
        /// Если значение свойства True, то не показываем кастомные меню.
        /// </summary>
        public bool UseStandardGridMenu { get; set; }

        /// <summary>
        /// Кол-во строк, привысив которое при отображении, пользователю будет выведена строка автофильтра
        /// При установке отрицательного значения - данная функционально отключается
        /// </summary>
        public long AutoShowAutoFilterRowWhenRowsCountMoreThan
        {
            get { return (long)GetValue(AutoShowAutoFilterRowWhenRowsCountMoreThanProperty); }
            set { SetValue(AutoShowAutoFilterRowWhenRowsCountMoreThanProperty, value); }
        }
        
        public static readonly DependencyProperty AutoShowAutoFilterRowWhenRowsCountMoreThanProperty = DependencyProperty.Register("AutoShowAutoFilterRowWhenRowsCountMoreThan", typeof(long), typeof(CustomTreeListControl), new PropertyMetadata(-1L));

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

        public event EventHandler ExpressionStyleOptionsChanged;
        public event EventHandler RestoredLayoutFromXml;

        /// <summary>
        /// Признак, что selected node имеет родителя
        /// </summary>
        public bool HaveChild
        {
            get { return (bool)GetValue(HaveChildProperty); }
            set { SetValue(HaveChildProperty, value); }
        }

        public static readonly DependencyProperty HaveChildProperty = DependencyProperty.Register("HaveChild", typeof(bool), typeof(CustomTreeListControl), new PropertyMetadata(OnHaveChildPropertyChanged));

        private static void OnHaveChildPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeListControl)d).HaveChild = (bool)e.NewValue;
        }
        #endregion Properties

        #region Methods
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

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "VisibleRowCount")
                return;

            TotalRowItemDataContextUpdate(TotalRowItemContentUpdate());
            TotalRowItemDataContextUpdate(_updateTimeRow, DateTime.Now);
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);

            if (_isLoaded || DesignerProperties.GetIsInDesignMode(this))
                return;

            _isLoaded = true;

            OnTotalRowPositionPropertyChanged(TotalRowPosition);
            RestoreLayoutInternal();

            RefreshAutoFilterVisible();

            if (Application.Current == null) 
                return;

            var themeName = ThemeManager.ActualApplicationThemeName;
            if (Application.Current.Resources.Contains(themeName))
            {
                View.AlternateRowBackground = Application.Current.Resources[themeName] as Brush;
                View.AlternationCount = 2;
                return;
            }

            if (!Application.Current.Resources.Contains(StyleKeys.EvenRowBrushKey)) 
                return;

            View.AlternateRowBackground = Application.Current.Resources[StyleKeys.EvenRowBrushKey] as Brush;
            View.AlternationCount = 2;
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

                var oldView = oldValue as TreeListView;
                if (oldView != null)
                {
                    oldView.RowDoubleClick -= OnViewRowDoubleClick;
                    ItemsSourceChanged -= OnItemsSourceChanged;
                }
            }

            var newView = newValue as TreeListView;
            if (newView != null)
            {
                newView.ShowGridMenu += OnShowGridMenu;
                newView.ShowingEditor += OnShowingEditor;
                newView.ShowFilterPopup += OnShowFilterPopup;

                ApplyPrefomanceSettings();

                // задаем специальный стиль для отображения автофильтра
                var newStyle = new Style(typeof(FilterCellContentPresenter), newView.AutoFilterRowCellStyle);
                newStyle.Setters.Add(new Setter(BackgroundProperty, AutoFilterRowDefaultColor));
                newView.AutoFilterRowCellStyle = newStyle;

                if (!UseStandardGridMenu)
                {
                    newView.ColumnMenuCustomizations.Add(new BarItemSeparator());

                    // добавляем кнопку для отображения автофильтра
                    _miShowAutoFilter = new BarButtonItem {Name = "showAutoFilterItem"};
                    _miShowAutoFilter.ItemClick += ShowAutoFilterOnItemClick;
                    newView.ColumnMenuCustomizations.Add(_miShowAutoFilter);

                    // добавляем кнопку для отображения панели суммирования
                    _miShowTotalSummary = new BarButtonItem {Name = "showTotalSummaryItem"};
                    _miShowTotalSummary.ItemClick += ShowTotalSummaryOnItemClick;
                    newView.ColumnMenuCustomizations.Add(_miShowTotalSummary);
                }

                newView.RowDoubleClick += OnViewRowDoubleClick;
                ItemsSourceChanged += OnItemsSourceChanged;
            }
        }
        
        private void OnShowGridMenu(object sender, GridMenuEventArgs e)
        {
            if (_miShowAutoFilter == null || UseStandardGridMenu)
                return;

            var view = sender as TreeListView;
            if (view == null)
                return;

            // отключаем контекстное меню для Autofilter
            if (e.MenuType == GridMenuType.RowCell)
            {
                var hi = view.CalcHitInfo((DependencyObject)e.TargetElement);
                if (hi.RowHandle == AutoFilterRowHandle)
                    e.Handled = true;
            }

            _miShowAutoFilter.Content = view.ShowAutoFilterRow
                ? StringResources.HideAutoFilterMenuItemContent
                : StringResources.ShowAutoFilterMenuItemContent;

            _miShowTotalSummary.Content = view.ShowTotalSummary
                ? StringResources.HideTotalSummaryContent
                : StringResources.ShowTotalSummaryContent;
        }

        private void ShowAutoFilterOnItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            // переключаем строчки
            View.ShowAutoFilterRow = !View.ShowAutoFilterRow;
        }

        private void ShowTotalSummaryOnItemClick(object sender, ItemClickEventArgs e)
        {
            View.ShowTotalSummary = !View.ShowTotalSummary;
        }

        private void OnViewRowDoubleClick(object sender, RowDoubleClickEventArgs e)
        {
            // отключаем командны для Autofilter
            if (e.HitInfo.RowHandle == AutoFilterRowHandle)
                e.Handled = true;
        }

        private void OnShowingEditor(object sender, TreeListShowingEditorEventArgs e)
        {
            var setts = e.Column.EditSettings as CustomGridSimpleLookupEditSettings;
            if (setts != null)
            {
                // такой вариант возможен , если DisplayMember = ValueMember
                e.Cancel = !setts.AllowEditAutofilter;
            }
        }

        private void ApplyPrefomanceSettings()
        {
            ApplyShowErrorMode();
            View.AutoWidth = false;
            View.AllowPerPixelScrolling = true;
            View.ScrollAnimationMode = ScrollAnimationMode.Linear;
            View.AllowCascadeUpdate = true;
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

        private void ApplyShowErrorMode()
        {
            View.ItemsSourceErrorInfoShowMode = ShowErrorMode;
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs itemsSourceChangedEventArgs)
        {
            RefreshAutoFilterVisible();
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

        public new void RestoreLayoutFromXml(string path)
        {
            if (!IsLoaded)
            {
                _xmlFileNamePath = path;
                return;
            }
            base.RestoreLayoutFromXml(path);
        }

        protected override DataViewBase CreateDefaultView()
        {
            return new CustomTreeListView();
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
            for (var i = 0; i < RowsCount; i++)
            {
                var obj = GetRow(i) as IKeyHandler;
                if (obj != null && key != null && key.Equals(obj.GetKey()))
                    return i;
            }
            return -1;
        }

        private void OnExpressionStyleOptionsChanged()
        {
            var h = ExpressionStyleOptionsChanged;
            if (h != null)
                h(this, EventArgs.Empty);
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
                //ApplyPrefomanceSettings();
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
                //ApplyPrefomanceSettings();
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

        private void OnRestoredLayoutFromXml()
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

        private void RestoreExpressionStyleOptionFromString(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            var xsl = new XmlSerializer(GetExpressionStyleOptionsType());
            using (var sr = new StringReader(layout))
            {
                using (var wr = XmlReader.Create(sr))
                {
                    ExpressionStyleOptions = (ExpressionStyle) xsl.Deserialize(wr);
                }
            }
        }

        private void SaveExpressionStyleOptionToXml(string path)
        {
            ExpressionStyleOptions.Version = ExpressionStyle.ActualVersion.ToString();
            var xsl = new XmlSerializer(GetExpressionStyleOptionsType());
            using (var writeFileStream = new StreamWriter(path))
            {
                xsl.Serialize(writeFileStream, ExpressionStyleOptions);
                writeFileStream.Close();
            }
        }

        private string SaveExpressionStyleOptionToString()
        {
            ExpressionStyleOptions.Version = ExpressionStyle.ActualVersion.ToString();
            var xsl = new XmlSerializer(GetExpressionStyleOptionsType());
            var sb = new StringBuilder();
            using (var wr = XmlWriter.Create(sb))
            {
                xsl.Serialize(wr, ExpressionStyleOptions);
            }
            return sb.ToString();
        }

        private void RefreshAutoFilterVisible()
        {
            // если включен режим автоматического появления автофильтра при большом кол-ве строк, то проверяем кол-во и отображаем
            if (AutoShowAutoFilterRowWhenRowsCountMoreThan >= 0 && AutoShowAutoFilterRowWhenRowsCountMoreThan < VisibleRowCount)
                IsAutoFilterVisible = true;
        }
        private void OnCopyingToClipboard(object sender, TreeListCopyingToClipboardEventArgs e)
        {
            if (!ByCheckCopyMode)
                return;

            ClipboardCopyMode = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                ? ClipboardCopyMode.IncludeHeader
                : ClipboardCopyMode.ExcludeHeader;
        }
        #endregion Methods

        #region IDisposable

        public void Dispose()
        {
            foreach (var dispEditSetts in Columns.Select(column => column.EditSettings).OfType<IDisposable>())
            {
                dispEditSetts.Dispose();
            }
        }
        #endregion
    }

    public class CustomTreeListColumn : TreeListColumn
    {
        public static readonly DependencyProperty SerializableNameProperty = DependencyProperty.Register("SerializableName", typeof(string), typeof(CustomTreeListColumn), new PropertyMetadata(default(string), OnSerializableNamePropertyChanged));
        public static readonly DependencyProperty BindingPathProperty = DependencyProperty.Register("BindingPath", typeof(string), typeof(CustomTreeListColumn), new PropertyMetadata(OnBindingPathPropertyChanged));

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

        static void OnSerializableNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeListColumn)d).Name = (string)e.NewValue;
        }

        static void OnBindingPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomTreeListColumn)d).OnBindingPathPropertyChanged(e.NewValue.To<string>());
        }

        private void OnBindingPathPropertyChanged(string newvalue)
        {
            if (string.IsNullOrEmpty(newvalue))
                return;

            Binding = new Binding { Path = new PropertyPath(newvalue),
                Mode = AllowEditing == DevExpress.Utils.DefaultBoolean.True ? BindingMode.TwoWay : BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged};
        }

        protected override void OnEditSettingsChanged(BaseEditSettings oldValue)
        {
            base.OnEditSettingsChanged(oldValue);
            
            //Если лукап меняем тип сортировки
            if (EditSettings is CustomGridSimpleLookupEditSettings)
                SortMode = ColumnSortMode.DisplayText;
        }
    }

    public class CustomTreeListView : TreeListView
    {
        public CustomTreeListView()
        {
            NavigationStyle = GridViewNavigationStyle.Cell;
        }
        //ShowSearchPanelMode="Always"
        //protected override void SearchControlKeyButtonUpProcessing(System.Windows.Input.KeyEventArgs e)
        //{
        //    base.SearchControlKeyButtonUpProcessing(e);
        //}
    }
}
