using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using wmsMLC.General.BL.Annotations;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Enums;
using wmsMLC.General.PL.WPF.ViewModels;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomSelectControlBase : ContentControl, INotifyPropertyChanged, ICommandSource
    {
        #region .  Constants & Fields  .
        public const int MaxRows = 9;
        private CustomListBoxEdit _list;
        private ContentPresenter _statusRow;
        #endregion .  Constants & Fields  .

        public CustomSelectControlBase()
        {
            DefaultStyleKey = typeof(CustomSelectControlBase);

            PreviousPageCommand = new DelegateCustomCommand(this, OnPreviousPage, CanPreviousPage);
            NextPageCommand = new DelegateCustomCommand(this, OnNextPage, CanNextPage);
            ItemDoubleClickCommand = new DelegateCustomCommand(this, OnItemDoubleClick, CanItemDoubleClick);
        }

        #region . Properties .
        public object EditValue
        {
            get { return GetValue(EditValueProperty); }
            set { SetValue(EditValueProperty, value); }
        }
        public static readonly DependencyProperty EditValueProperty = DependencyProperty.Register("EditValue", typeof(object), typeof(CustomSelectControlBase), new PropertyMetadata(OnEditValueChanged));

        private static void OnEditValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectControlBase)d).OnEditValueChanged();
        }

        public List<SelectListItem> ItemsSource
        {
            get { return (List<SelectListItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<SelectListItem>), typeof(CustomSelectControlBase), new PropertyMetadata(OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectControlBase)d).OnItemsSourceChanged();
        }

        public int MaxRowsOnPage
        {
            get { return (int)GetValue(MaxRowsOnPageProperty); }
            set { SetValue(MaxRowsOnPageProperty, value); }
        }
        public static readonly DependencyProperty MaxRowsOnPageProperty = DependencyProperty.Register("MaxRowsOnPage", typeof(int), typeof(CustomSelectControlBase), new PropertyMetadata(MaxRows, OnMaxRowsOnPageChanged));

        private static void OnMaxRowsOnPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CustomSelectControlBase)d).OnMaxRowsOnPageChanged();
        }

        private void OnMaxRowsOnPageChanged()
        {
            if (MaxRowsOnPage <= 0 || MaxRowsOnPage > MaxRows)
                MaxRowsOnPage = MaxRows;
        }

        public bool UseFunctionKeys
        {
            get { return (bool)GetValue(UseFunctionKeysProperty); }
            set { SetValue(UseFunctionKeysProperty, value); }
        }
        public static readonly DependencyProperty UseFunctionKeysProperty = DependencyProperty.Register("UseFunctionKeys", typeof(bool), typeof(CustomSelectControlBase));

        public bool ParentKeyPreview
        {
            get { return (bool)GetValue(ParentKeyPreviewProperty); }
            set { SetValue(ParentKeyPreviewProperty, value); }
        }
        public static readonly DependencyProperty ParentKeyPreviewProperty = DependencyProperty.Register("ParentKeyPreview", typeof(bool), typeof(CustomSelectControlBase));

        public DataTemplate StatusContentTemplate
        {
            get { return (DataTemplate)GetValue(StatusContentTemplateProperty); }
            set { SetValue(StatusContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty StatusContentTemplateProperty = DependencyProperty.Register("StatusContentTemplate", typeof(DataTemplate), typeof(CustomSelectControlBase));

        public Visibility StatusContentVisibility
        {
            get { return (Visibility)GetValue(StatusContentVisibilityProperty); }
            set { SetValue(StatusContentVisibilityProperty, value); }
        }
        public static readonly DependencyProperty StatusContentVisibilityProperty = DependencyProperty.Register("StatusContentVisibility", typeof(Visibility), typeof(CustomSelectControlBase), new PropertyMetadata(Visibility.Collapsed));

        public ControlTemplate BorderTemplate
        {
            get { return (ControlTemplate) GetValue(BorderTemplateProperty); }
            set { SetValue(BorderTemplateProperty, value); }
        }
        public static readonly DependencyProperty BorderTemplateProperty = DependencyProperty.Register("BorderTemplate", typeof(ControlTemplate), typeof(CustomSelectControlBase));

        public string TotalRowsDisplayFormat
        {
            get { return (string)GetValue(TotalRowsDisplayFormatProperty); }
            set { SetValue(TotalRowsDisplayFormatProperty, value); }
        }
        public static readonly DependencyProperty TotalRowsDisplayFormatProperty = DependencyProperty.Register("TotalRowsDisplayFormat", typeof(string), typeof(CustomSelectControlBase));

        public string TotalRowsInfo
        {
            get { return (string)GetValue(TotalRowsInfoProperty); }
            set { SetValue(TotalRowsInfoProperty, value); }
        }
        public static readonly DependencyProperty TotalRowsInfoProperty = DependencyProperty.Register("TotalRowsInfo", typeof(string), typeof(CustomSelectControlBase));

        private List<SelectListItem> _actualItemsSource;
        public List<SelectListItem> ActualItemsSource
        {
            get { return _actualItemsSource ?? (_actualItemsSource = new List<SelectListItem>()); }
            set
            {
                if (_actualItemsSource == value)
                    return;
                _actualItemsSource = value;
                OnPropertyChanged("ActualItemsSource");
            }
        }

        private SelectListItem _selectedItem;
        public SelectListItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value)
                    return;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        /// <summary>
        /// Внешний источник данных.
        /// </summary>
        public IList ExternalItemsSource { get; set; }

        public string ValueMember { get; set; }
        public string DisplayMember { get; set; }

        protected int CurrentIndex { get; set; }

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
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(CustomSelectControl));

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
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(CustomSelectControl));

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
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(CustomSelectControl));
        #endregion . ICommandSource .
        #endregion . Properties .

        #region . Command .
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand ItemDoubleClickCommand { get; private set; }

        private bool CanPreviousPage()
        {
            return TotalRowCount() > 0 && CurrentIndex > 0;
        }

        private bool OnPreviousPageInternal()
        {
            if (!CanPreviousPage())
                return false;

            CurrentIndex -= MaxRowsOnPage;
            if (CurrentIndex < 0)
            {
                CurrentIndex = 0;
                return false;
            }
            ScrollingItems();
            return true;
        }

        private void OnPreviousPage()
        {
            OnPreviousPageInternal();
        }

        private bool CanNextPage()
        {
            var count = TotalRowCount();
            return count > 0 && CurrentIndex + MaxRowsOnPage < count;
        }

        private bool OnNextPageInternal()
        {
            if (!CanNextPage())
                return false;

            var currentindex = CurrentIndex;
            CurrentIndex += MaxRowsOnPage;
            var count = TotalRowCount();
            if (CurrentIndex > count)
            {
                CurrentIndex = currentindex;
                return false;
            }
            ScrollingItems();
            return true;
        }

        private void OnNextPage()
        {
            OnNextPageInternal();
        }

        private bool CanItemDoubleClick()
        {
            return SelectedItem != null;
        }

        private void OnItemDoubleClick()
        {
            if (!CanItemDoubleClick())
                return;

            ExecuteCommandSource(this);
        }
        #endregion . Command .

        #region . Methods .
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var grid = GetTemplateChild("LayoutRoot") as Grid;
            if (grid != null)
                grid.DataContext = this;

            _list = GetTemplateChild("list") as CustomListBoxEdit;
            if (_list != null)
            {
                _list.SelectedIndexChanged -= OnSelectedIndexChanged;
                _list.SelectedIndexChanged += OnSelectedIndexChanged;
            }

            _statusRow = GetTemplateChild("StatusRow") as ContentPresenter;
            if (_statusRow != null)
            {
                _statusRow.Loaded -= OnLoadedStatusRow;
                _statusRow.Loaded += OnLoadedStatusRow;
            }
        }

        private void OnLoadedStatusRow(object sender, RoutedEventArgs e)
        {
            if (_statusRow == null)
                return;

            _statusRow.Loaded -= OnLoadedStatusRow;
            if (_statusRow.DataContext == null)
                _statusRow.DataContext = this;

        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            PreviewHotKey(e);
            PreviewItemHotKey(e);
            base.OnPreviewKeyDown(e);
        }

        public void PreviewHotKey(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            e.Handled = PreviewHotKey(e.Key);
        }

        public bool PreviewHotKey(Key hotKey, Key hotKey2)
        {
            var result = PreviewHotKey(hotKey);
            return result || PreviewHotKey(hotKey2);
        }

        public bool PreviewHotKey(Key key)
        {
            var handled = false;
            if (Visibility == Visibility.Visible && IsEnabled)
            {
                Func<int> getRowcountFunc = () => _list == null ? 0 : _list.RowsCount;
                var rowcount = getRowcountFunc();
                Action<int, int> handler = (indx, rows) =>
                {
                    if (rows == 0 || indx >= rows)
                        return;

                    _list.SetSelected(indx);
                    handled = true;
                };

                switch (key)
                {
                    case Key.Left:
                        if (_list != null && OnPreviousPageInternal() && (rowcount = getRowcountFunc()) > 0)
                            _list.SelectedIndex = rowcount - 1;
                        handled = true;
                        break;
                    case Key.Right:
                        if (_list != null && OnNextPageInternal() && getRowcountFunc() > 0)
                            _list.SelectedIndex = 0;
                        handled = true;
                        break;
                    case Key.Up:
                        if (_list != null && _list.SelectedIndex >= 0)
                        {
                            var index = _list.SelectedIndex - 1;
                            if (index < 0)
                                return false;

                            handler(index, rowcount);
                        }
                        break;
                    case Key.Down:
                        if (_list != null && _list.SelectedIndex >= 0)
                        {
                            var index = _list.SelectedIndex + 1;
                            if (index >= rowcount)
                                return false;

                            handler(index, rowcount);
                        }
                        break;
                }
            }
            return handled;
        }

        public void PreviewItemHotKey(KeyEventArgs e)
        {
            if (e.Handled)
                return;

            if (SelectedItem != null && e.Key == Key.Enter)
            {
                ExecuteCommandSource(this);
                return;
            }

            var selectedItem = ActualItemsSource.FirstOrDefault(p => p.HotKey == e.Key);
            if (selectedItem != null)
            {
                SelectedItem = selectedItem;
                if (_list != null && _list.SelectedIndex >= 0)
                    _list.SetSelected(_list.SelectedIndex);
                UpdateEditValue();
                ExecuteCommandSource(this);
                e.Handled = true;
            }
        }

        protected virtual void OnSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            UpdateEditValue();
        }

        protected virtual void UpdateEditValue()
        {
            EditValue = null;
            if (SelectedItem == null || SelectedItem.Value == null)
                return;

            EditValue = SelectedItem.Value.Id;
        }

        protected static void ExecuteCommandSource(ICommandSource commandSource)
        {
            var command = commandSource.Command;
            if (command != null)
            {
                var commandParameter = commandSource.CommandParameter;
                if (command.CanExecute(commandParameter))
                    command.Execute(commandParameter);
            }
        }

        protected int TotalRowCount()
        {
            return ItemsSource == null ? 0 : ItemsSource.Count;
        }

        protected virtual void OnEditValueChanged()
        {
        }

        protected virtual void OnItemsSourceChanged()
        {
            ActualItemsSource = null;
            CurrentIndex = 0;

            try
            {
                if (ItemsSource == null || ItemsSource.Count == 0)
                    return;

                if (EditValue != null)
                {
                    var item = FindItem(ItemsSource);
                    if (item != null)
                    {
                        var index = ItemsSource.IndexOf(item);
                        if (index >= 0)
                        {
                            var currentIndex = index/MaxRowsOnPage*MaxRowsOnPage;
                            var rows = TotalRowCount();
                            if (currentIndex > rows)
                                currentIndex = rows - 1;
                            CurrentIndex = currentIndex;
                        }
                    }
                }

                ScrollingItems();
                if (ActualItemsSource != null)
                {
                    if (EditValue != null)
                        SelectedItem = FindItem(ActualItemsSource);

                    if (SelectedItem == null)
                        SelectedItem = ActualItemsSource.FirstOrDefault();

                    if (EditValue == null && SelectedItem != null && SelectedItem.Value != null)
                        EditValue = SelectedItem.Value.Id;
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(TotalRowsDisplayFormat))
                    TotalRowsInfo = string.Format(TotalRowsDisplayFormat, TotalRowCount());
            }
        }

        protected SelectListItem FindItem(List<SelectListItem> source)
        {
            if (source == null)
                return null;
            return source.FirstOrDefault(p => p.Value != null && Equals(p.Value.Id, EditValue));
        }

        protected void ScrollingItems()
        {
            if (ItemsSource == null)
                return;

            var items = new List<SelectListItem>(ItemsSource.Select(p => new { p, index = ItemsSource.IndexOf(p) })
                .Where(s => s.index >= CurrentIndex && s.index < CurrentIndex + MaxRowsOnPage).Select(s => s.p));

            UpdateItemProperty(items);
            ActualItemsSource = items;
        }

        protected void UpdateItemProperty(List<SelectListItem> itemsSource)
        {
            if (itemsSource == null)
                return;

            foreach (var p in itemsSource.Where(p => p != null && p.Value != null))
            {
                var index = itemsSource.IndexOf(p) + 1;
                if (p.HotKey == Key.None)
                {
                    p.HotKey = string.Format("{0}{1}", UseFunctionKeys ? "F" : "D", index).To(Key.None);
                }

                p.DisplayText = string.Format("{0} {1}", EnumFormatters.Format(p.HotKey), p.Value.Name);
            }
        }
        #endregion . Methods .

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

        public class SelectListItem : ViewModelBase
        {
            public int Id { get; set; }
            public Key HotKey { get; set; }
            public string DisplayText { get; set; }

            private ListItemStyle _style;
            public ListItemStyle Style
            {
                get { return _style; }
                set
                {
                    if (_style == value)
                        return;
                    _style = value;
                    OnPropertyChanged("Style");
                }
            }

            public SelectViewItem Value { get; set; }
        }

        public class SelectViewItem
        {
            public object Id { get; set; }
            public string Name { get; set; }
        }
    }
}
