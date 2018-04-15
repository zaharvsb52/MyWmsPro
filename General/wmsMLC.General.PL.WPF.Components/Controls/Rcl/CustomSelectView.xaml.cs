using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Xpf.Grid;
using wmsMLC.General.BL.Annotations;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public partial class CustomSelectView : INotifyPropertyChanged, ICommandSource
    {
        public CustomSelectView()
        {
            ShowMenu = true;
            AllowAutoShowAutoFilterRow = true;
            ShowTotalRow = true;
            InitializeComponent();

            DisplaySetting = SettingDisplay.List;

            SelectCommand = new DelegateCustomCommand<ValueDataField>(this, OnSelect, CanSelect);
            FindCommand = new DelegateCustomCommand(this, OnFind, CanFind);
            AutoFilterCommand = new DelegateCustomCommand(this, OnAutoFilter, CanAutoFilter);
            ClearFilterCommand = new DelegateCustomCommand(this, OnClearFilter, CanClearFilter);

            Loaded += Onloaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
            LayoutRoot.DataContext = this;
        }
        #region . Properties .
        public Type ItemsSourceType
        {
            get { return (Type) GetValue(ItemsSourceTypeProperty); }
            set { SetValue(ItemsSourceTypeProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceTypeProperty = DependencyProperty.Register("ItemsSourceType", typeof(Type), typeof(CustomSelectView));

        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(CustomSelectView), new PropertyMetadata(OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectView) d).OnSourceChanged();
        }

        public object EditValue
        {
            get { return GetValue(EditValueProperty); }
            set { SetValue(EditValueProperty, value); }
        }
        public static readonly DependencyProperty EditValueProperty = DependencyProperty.Register("EditValue", typeof(object), typeof(CustomSelectView));

        private int _autoShowAutoFilterRowWhenRowsCountMoreThan;
        public int AutoShowAutoFilterRowWhenRowsCountMoreThan
        {
            get { return _autoShowAutoFilterRowWhenRowsCountMoreThan; }
            set
            {
                if (_autoShowAutoFilterRowWhenRowsCountMoreThan == value)
                    return;
                _autoShowAutoFilterRowWhenRowsCountMoreThan = value;
                OnPropertyChanged("AutoShowAutoFilterRowWhenRowsCountMoreThan");
            }
        }

        private string _totalRowItemAdditionalInfo;
        public string TotalRowItemAdditionalInfo
        {
            get { return _totalRowItemAdditionalInfo; }
            set
            {
                if (_totalRowItemAdditionalInfo == value) 
                    return;
                _totalRowItemAdditionalInfo = value;
                OnPropertyChanged("TotalRowItemAdditionalInfo");
            }
        }

        private ObservableCollection<DataField> _fields;
        public ObservableCollection<DataField> Fields
        {
            get { return _fields; }
            set
            {
                if (value == _fields) 
                    return;
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        private string _findMenuText;
        public string FindMenuText
        {
            get { return _findMenuText; }
            set
            {
                if (_findMenuText == value)
                    return;
                _findMenuText = value;
                OnPropertyChanged("FindMenuText");
            }
        }

        private bool _verifyColumnsSourceChanged;

        public bool VerifyColumnsSourceChanged
        {
            get { return _verifyColumnsSourceChanged; }
            set
            {
                if (_verifyColumnsSourceChanged == value)
                    return;
                _verifyColumnsSourceChanged = value;
                OnPropertyChanged("VerifyColumnsSourceChanged");
            }
        }

        private bool _showMenu;
        public bool ShowMenu
        {
            get { return _showMenu; }
            set
            {
                if (_showMenu == value)
                    return;
                _showMenu = value;
                OnShowMenuPropertyChanged();
            }
        }

        private bool _showTotalRow;
        public bool ShowTotalRow
        {
            get { return _showTotalRow; }
            set
            {
                if (_showTotalRow == value)
                    return;
                _showTotalRow = value;
                OnPropertyChanged("ShowTotalRow");
            }
        }

        private bool _showSelectMenuItem;
        public bool ShowSelectMenuItem
        {
            get { return _showMenu && _showSelectMenuItem; }
            set
            {
                if (_showSelectMenuItem == value)
                    return;
                _showSelectMenuItem = value;
                OnShowMenuPropertyChanged();
            }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(object), typeof(CustomSelectView));

        public string LayoutValue
        {
            get { return (string) GetValue(LayoutValueProperty); }
            set { SetValue(LayoutValueProperty, value); }
        }
        public static readonly DependencyProperty LayoutValueProperty = DependencyProperty.Register("LayoutValue", typeof(string), typeof(CustomSelectView), new PropertyMetadata(OnLayoutValueChanged));

        private static void OnLayoutValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectView)d).OnLayoutValueChanged();
        }

        public bool ShowAutoFilterRow
        {
            get { return (bool)GetValue(ShowAutoFilterRowProperty); }
            set { SetValue(ShowAutoFilterRowProperty, value); }
        }
        public static readonly DependencyProperty ShowAutoFilterRowProperty = DependencyProperty.Register("ShowAutoFilterRow", typeof(bool), typeof(CustomSelectView), new PropertyMetadata(OnShowAutoFilterRowChanged));

        private static void OnShowAutoFilterRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectView) d).OnShowAutoFilterRowChanged();
        }

        private void OnShowAutoFilterRowChanged()
        {
            var view = RclGridControl.View as TableView;
            if (view == null)
                return;
            view.ShowAutoFilterRow = ShowAutoFilterRow;
        }

        private static readonly DependencyProperty ShowAutoFilterRowBaseProperty = DependencyProperty.Register("ShowAutoFilterRowBase", typeof(bool), typeof(CustomSelectView), new PropertyMetadata(OnShowAutoFilterRowBaseChanged));

        private static void OnShowAutoFilterRowBaseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectView)d).OnShowAutoFilterRowBaseChanged();
        }

        private void OnShowAutoFilterRowBaseChanged()
        {
            var view = RclGridControl.View as TableView;
            if (view != null && ShowAutoFilterRow != view.ShowAutoFilterRow)
            {
                if (RclGridControl.IsRestoringLayoutFromXml && DoNotAllowUpdateShowAutoFilterRowFromXml)
                    view.ShowAutoFilterRow = ShowAutoFilterRow;
                else
                    ShowAutoFilterRow = view.ShowAutoFilterRow;
            }
            UpdateFindItemMenu();
            UpdateViewModelProperties();
        }

        public bool DoNotAllowUpdateShowAutoFilterRowFromXml { get; set; }

        public string LookUpCodeEditor { get; set; }
        public string LookUpCodeEditorFilterExt { get; set; }
        public SettingDisplay DisplaySetting { get; set; }

        public int? MaxRows { get; set; }

        public bool IsWfDesignMode
        {
            get { return RclGridControl.IsWfDesignMode; }
            set { RclGridControl.IsWfDesignMode = value; }
        }

        private bool IsDataLoading { get; set; }

        /// <summary>
        /// Автоматическое включение строки автофильтра.
        /// </summary>
        public bool AllowAutoShowAutoFilterRow { get; set; }

        public bool DoNotActionOnEnterKey { get; set; }
        public string[] BestFitColumnNames { get; set; }

        #region . Commands .

        public ICommand SelectCommand { get; private set; }
        public ICommand FindCommand { get; private set; }
        public ICommand ClearFilterCommand { get; private set; }
        public ICommand AutoFilterCommand { get; private set; }

        private bool CanAutoFilter()
        {
            return Source != null;
        }

        private void OnAutoFilter()
        {
            if (!CanAutoFilter())
                return;

            ShowAutoFilterRow = !ShowAutoFilterRow;
            if (!ShowAutoFilterRow && RclGridControl.View.ActualShowSearchPanel)
                RclGridControl.View.Commands.HideSearchPanel.Execute(null);
        }

        private bool CanSelect(ValueDataField parameter)
        {
            //return RclGridControl.SelectedItem != null && ShowSelectMenuItem;
            return RclGridControl.SelectedItem != null && !DoNotActionOnEnterKey;
        }

        private void OnSelect(ValueDataField parameter)
        {
            if (!CanSelect(parameter))
                return;

            CommandParameter = ValueDataFieldConstants.CreateDefaultParameter(ValueDataFieldConstants.Value);
            ExecuteCommandSource(this);
        }

        private bool CanFind()
        {
            return Source != null;
        }

        private void OnFind()
        {
            if (!CanFind())
                return;

            if (RclGridControl.View.ActualShowSearchPanel)
                RclGridControl.View.Commands.HideSearchPanel.Execute(null);
            else
                RclGridControl.View.Commands.ShowSearchPanel.Execute(false);
        }

        private bool CanClearFilter()
        {
            //return ShowAutoFilterRow;
            return true;
        }

        private void OnClearFilter()
        {
            if (!CanClearFilter())
                return;

            RclGridControl.FilterCriteria = null;
            RclGridControl.View.Commands.ClearFilter.Execute(null);

            if (RclGridControl.View.ActualShowSearchPanel)
            {
                try
                {
                    RclGridControl.BeginDataUpdate();
                    RclGridControl.View.Commands.HideSearchPanel.Execute(null);
                    RclGridControl.View.Commands.ShowSearchPanel.Execute(false);
                }
                finally
                {
                    RclGridControl.EndDataUpdate();
                }
            }
        }

        #endregion . Commands .

        #region . ICommandSource .
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CustomSelectView));

        public object CommandParameter
        {
            get
            {
                return GetValue(CommandParameterProperty);
            }
            set
            {
                SetValue(CommandParameterProperty, value);
            }
        }
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(CustomSelectView));

        public IInputElement CommandTarget
        {
            get
            {
                return (IInputElement)GetValue(CommandTargetProperty);
            }
            set
            {
                SetValue(CommandTargetProperty, value);
            }
        }

        public IEnumerable<FormatConditionInfo> FormatConditions { get; set; }

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(CustomSelectView));
        #endregion . ICommandSource .
        #endregion . Properties .

        #region . Methods .

        public override void OnApplyTemplate()
        {  
            base.OnApplyTemplate();
            BindingOperations.SetBinding(this, ShowAutoFilterRowBaseProperty, new Binding("ShowAutoFilterRow") { Source = RclGridControl.View });
        }

        private void Onloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Onloaded;

            //if (IsWfDesignMode && (Source == null || string.IsNullOrEmpty(LookUpCodeEditor)))
            //    return;

            OnDataLoaded();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;
            DataContextChanged -= OnDataContextChanged;

            Menu.Menu.Clear();
        }

        private void OnSourceChanged()
        {
            if (IsDataLoading || !IsWfDesignMode)
                return;

            OnDataLoaded();
        }

        private async void OnDataLoaded()
        {
            try
            {
                WaitIndicatorVisible = true;
                IsDataLoading = true;

                if (!string.IsNullOrEmpty(LookUpCodeEditor))
                    Source = await PrepareItemsSourceByLookupCodeAsync();

                if (Fields == null || !Fields.Any())
                    GetDataFields();
                RclGridControl.ItemsSource = Source;

                UpdateViewModelProperties();
                UpdateFindItemMenu();
                UpdateFormatConditions();

                SetSearchPanelHighlightResults();
                RclGridControl.BackgroundFocus();
            }
            finally
            {
                IsDataLoading = false;
                WaitIndicatorVisible = false;
            }
        }

        private void UpdateFormatConditions()
        {
            // переносим форматирование
            if (FormatConditions != null)
            {
                var tableView = RclGridControl.View as TableView;
                if (tableView != null)
                {
                    tableView.FormatConditions.Clear();
                    foreach (var info in FormatConditions)
                    {
                        var condition = new FormatCondition();
                        condition.FieldName = info.FieldName;
                        condition.Expression = info.Expression;
                        condition.ApplyToRow = info.ApplyToRow;
                        condition.PredefinedFormatName = info.PredefinedFormatName;
                        tableView.FormatConditions.Add(condition);
                    }
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateViewModelProperties();
        }

        private void OnLayoutValueChanged()
        {
            if (!string.IsNullOrEmpty(LayoutValue))
                RclGridControl.RestoreLayoutFromString(LayoutValue);
        }

        private void OnRestoredLayoutFromXml(object sender, EventArgs e)
        {
            if (DoNotAllowUpdateShowAutoFilterRowFromXml)
            {
                var view = RclGridControl.View as TableView;
                if (view != null && view.ShowAutoFilterRow != ShowAutoFilterRow) 
                    OnShowAutoFilterRowChanged();
            }

            UpdateViewModelProperties();
            UpdateFindItemMenu();
            SetSearchPanelHighlightResults();
        }

        private void UpdateViewModelProperties()
        {
            var model = DataContext as IDictionary<string, object>;
            if (model != null && model.ContainsKey(ValueDataFieldConstants.Properties))
                ((IDictionary<string, object>)model[ValueDataFieldConstants.Properties])[
                    ValueDataFieldConstants.ShowAutoFilterRow] = ShowAutoFilterRow;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
                return;

            if (!DoNotActionOnEnterKey && e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None && RclGridControl.SelectedItem != null)
            {
                e.Handled = true;
                OnSelect(ValueDataFieldConstants.CreateDefaultParameter(ValueDataFieldConstants.Value));
                return;
            }

            if ((AllowAutoShowAutoFilterRow || ShowAutoFilterRow) && IsNotFunctionalKey(e.Key))
            {
                if (!ShowAutoFilterRow)
                    ShowAutoFilterRow = true;

                RclGridControl.SetFocusRow(DataControlBase.AutoFilterRowHandle);
            }
        }

        private bool IsNotFunctionalKey(Key key)
        {
            var keys = key.ToString().ToUpper();
            return (Keyboard.Modifiers == ModifierKeys.None &&
                key != Key.Enter && key != Key.Escape && 
                key != Key.Up && key != Key.PageUp && 
                key != Key.Down && key != Key.PageDown &&
                key != Key.Left && key != Key.Right &&
                key != Key.Tab &&
                (keys.Length == 1 || (keys.Length > 1 && !keys.StartsWith("F"))));
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            RclGridControl.Focus();
        }

        private void SetSearchPanelHighlightResults()
        {
            //Настройки ActualShowSearchPanel
            RclGridControl.View.SearchPanelHighlightResults = false;
            RclGridControl.View.ShowSearchPanelCloseButton = false;
        }

        /// <summary>
        /// Получаем источник данных, если задан LookUpCodeEditor.
        /// </summary>
        private Task<object> PrepareItemsSourceByLookupCodeAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                //Thread.Sleep(10000);

                if (string.IsNullOrEmpty(LookUpCodeEditor))
                    return null;

                var lookupInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
                var filtertxt = lookupInfo.Filter;
                string filter0;
                LookupHelper.InitializeVarFilter(filtertxt, out filter0);
                var filters = new List<string> { filter0, LookUpCodeEditorFilterExt };
                var filter = string.Join(" AND ", filters.Where(p => !string.IsNullOrEmpty(p)).Select(p => p));

                if (MaxRows.HasValue && MaxRows.Value > 0)
                    filter = string.Format("{0}ROWNUM <= {1}", string.IsNullOrEmpty(filter) ? string.Empty : " AND ",
                        MaxRows.Value);

                Dispatcher.Invoke(new Action(() => { ItemsSourceType = LookupHelper.GetItemSourceType(lookupInfo); }));

                //var valueMember = lookUp.ObjectLookupPkey;
                //var displayMember = lookUp.ObjectLookupDisplay;

                // using не делаем - можем прибить singleton manager
                var mgr = LookupHelper.GetItemSourceManager(lookupInfo);

                object result = mgr.GetFiltered(filter, GetModeEnum.Partial);
                return result;
            });
        }

        private void GetDataFields()
        {
            Fields = DataFieldHelper.Instance.GetDataFields(ItemsSourceType, DisplaySetting);
        }

        private void OnItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            IsDataLoading = false;

            if (BestFitColumnNames != null)
            {
                foreach (var p in BestFitColumnNames.Where(p => !string.IsNullOrEmpty(p)))
                {
                    var col = RclGridControl.Columns[p];
                    if (col == null)
                        continue;
                    RclGridControl.BestFitColumn(col);
                }
            }

            if (RclGridControl.RowsCount <= 0)
            {
                SelectedItem = null;
                SetEditValue();
                return;
            }

            RclGridControl.UnselectAll();

            var index = 0;
            if (EditValue != null)
            {
                index = RclGridControl.IndexOf(EditValue);
                if (index < 0)
                {
                    index = 0;
                    EditValue = null;
                }
            }

            RclGridControl.SelectItem(index);
            RclGridControl.View.FocusedRowHandle = index;

            var column = RclGridControl.Columns.FirstOrDefault(p => p.VisibleIndex == 0);
            if (column != null && (RclGridControl.CurrentColumn == null || RclGridControl.CurrentColumn.VisibleIndex != column.VisibleIndex))
            {
                RclGridControl.CurrentColumn = column;
                //((TableView) RclGridControl.View).SelectCell(index, column);
                //RclGridControl.View.MovePrevCell();
            }

            if (EditValue == null)
                SetEditValue();
        }

        private void OnSelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            OnPropertyChanged(string.Empty);
            SelectedItem = RclGridControl.SelectedItem;
            if (IsDataLoading)
                return;

            SetEditValue();
        }

        public string SaveLayoutToString()
        {
            return RclGridControl.SaveLayoutToString();
        }

        private void SetEditValue()
        {
            if (SelectedItem == null)
            {
                EditValue = null;
                return;
            }

            var keyHandler = SelectedItem as IKeyHandler;
            if (keyHandler != null)
            {
                var key = keyHandler.GetKey();
                if (!Equals(key, EditValue))
                    EditValue = keyHandler.GetKey();
            }
            else
            {
                EditValue = SelectedItem;
            }
        }

        private void UpdateFindItemMenu()
        {
            FindMenuText = GetFindMenuItemCaption();
        }

        private void OnShowMenuPropertyChanged()
        {
            OnPropertyChanged("ShowMenu");
            OnPropertyChanged("ShowSelectMenuItem");
        }

        private static void ExecuteCommandSource(ICommandSource commandSource)
        {
            var command = commandSource.Command;
            if (command != null)
            {
                var commandParameter = commandSource.CommandParameter;
                if (command.CanExecute(commandParameter))
                    command.Execute(commandParameter);
            }
        }

        public void AddFooterMenuItem(ValueDataField item, int index, double? fontSize, bool isWfDesignMode)
        {
            MenuHelper.AddFooterMenuItem(Menu, item, 0, new DelegateCustomCommand<ValueDataField>(OnFooterMenuItemClick), fontSize ?? 0, isWfDesignMode);
        }

        public void AddFooterMenuItems(ValueDataField[] items, double? fontSize, bool isWfDesignMode)
        {
            var index = 0;
            foreach (var item in items)
            {
                AddFooterMenuItem(item, index++, fontSize, isWfDesignMode);
            }
        }

        private void OnFooterMenuItemClick(ValueDataField parameter)
        {
            CommandParameter = parameter;
            ExecuteCommandSource(this);
        }

        private string GetFindMenuItemCaption()
        {
            return ShowAutoFilterRow ? "Скрыть" : "Поиск";
        }

        #region .  INotifyPropertyChanged  .
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion .  INotifyPropertyChanged  .
        #endregion . Methods .
    }
}
