using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class FooterMenu : ContentControl, IFooterMenu
    {
        #region .  Constants & Fields  .
        private int _beginIndex;
        private int _lastEndIndex;
        private int _currentPage;
        private readonly int[] _pageIndexes;

        private int _maxRows;
        private int _maxColumns;

        private Grid _grid;
        #endregion .  Constants & Fields  .

        public FooterMenu()
        {
            DefaultStyleKey = typeof(FooterMenu);
            NavigationDirectionOnGotFocus = FocusNavigationDirection.Previous;
            _currentPage = 0;
            _beginIndex = 0;
            // более чем достаточно
            _pageIndexes = new int[1000];
        }

        #region . Properties .

        public static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof(int), typeof(FooterMenu), new FrameworkPropertyMetadata(0), IsIntValueNotNegative);

        public static readonly DependencyProperty RowProperty = DependencyProperty.RegisterAttached("Row", typeof(int), typeof(FooterMenu), new FrameworkPropertyMetadata(0), IsIntValueNotNegative);

        private ObservableCollection<CustomButton> _menu;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ObservableCollection<CustomButton> Menu
        {
            get { return _menu ?? (_menu = new ObservableCollection<CustomButton>()); }
        }

        public FocusNavigationDirection NavigationDirectionOnGotFocus { get; set; }

        private ICommand MoveNextCommand { get; set; }

        private ICommand MoveBackCommand { get; set; }

        private int MaxMenuItemsCount
        {
            get { return _maxRows*_maxColumns; }
        }

        #endregion . Properties .

        #region . Methods .
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Menu == null || Menu.Count == 0)
                return;

            _grid = GetTemplateChild("LayoutRoot") as Grid;
            if (_grid == null)
                throw new Exception("Can't find element with name 'LayoutRoot'.");

            _maxColumns = _grid.ColumnDefinitions.Count;
            _maxRows = _grid.RowDefinitions.Count;

            AddMenu();
            Menu.CollectionChanged -= OnCollectionChanged;
            Menu.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddMenu();
        }

        private void AddMenu()
        {
            if (_grid == null)
                return;

            _grid.Children.Clear();
            var maxcolindex = _maxColumns - 1;
            if (maxcolindex < 0)
                maxcolindex = 0;

            var count = Menu.Count;
            var isoverall = count > MaxMenuItemsCount;

            var table = new int[_maxRows, _maxColumns];
            var keyTable = new List<Key>();
            _pageIndexes[_currentPage] = _beginIndex;
            var begin = _beginIndex;
            var overall = count - begin;
            var btnIndex = 0;

            // можно идти вперед
            if (isoverall && overall >= MaxMenuItemsCount)
            {
                btnIndex++;
                MoveNextCommand = new DelegateCommand(() =>
                {
                    _currentPage++;
                    _beginIndex = ++_lastEndIndex;
                    AddMenu();
                }, true);
                var next = new CustomButton()
                {
                    ShowHotKeyInTitle = false,
                    Text = ">>",
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HotKey = Key.Right,
                    HotKey2 = Key.Right,
                    Command = MoveNextCommand,
                    CommandParameter = null,
                    IsEnabled = true,
                    Visibility = Visibility.Visible,
                    FontSize = FontSize,
                    TransferHotKeyToControls = false,
                    Margin = new Thickness(0, 2, 0, 0),
                    Focusable = false
                };
                // займем ячейку
                table[2, 1] = 1;
                Grid.SetColumn(next, 1);
                Grid.SetRow(next, 2);
                _grid.Children.Add(next);
            }
            // можно идти назад
            if (begin != 0)
            {
                btnIndex++;
                MoveBackCommand = new DelegateCommand(() =>
                {
                    _currentPage--;
                    if (_currentPage < 0)
                        _currentPage = 0;
                    _beginIndex = _pageIndexes[_currentPage];
                    AddMenu();
                }, true);
                var back = new CustomButton()
                {
                    ShowHotKeyInTitle = false,
                    Text = "<<",
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HotKey = Key.Left,
                    HotKey2 = Key.Left,
                    Command = MoveBackCommand,
                    CommandParameter = null,
                    IsEnabled = true,
                    Visibility = Visibility.Visible,
                    FontSize = FontSize,
                    TransferHotKeyToControls = false,
                    Margin = new Thickness(0, 2, 2, 0),
                    Focusable = false
                };
                // займем ячейку
                table[2, 0] = 1;
                Grid.SetColumn(back, 0);
                Grid.SetRow(back, 2);
                _grid.Children.Add(back);
            }

            var end = begin + MaxMenuItemsCount - btnIndex;
            if (end > count)
                end = count;

            // зарезервируем кнопки диапазона
            keyTable.AddRange(Menu.Skip(begin).Take(end - begin).Select(i => i.HotKey));

            // проход постранично
            for (var i = begin; i < end; i++)
            {
                var menu = Menu[i];

                // найдем свободную ячейку
                var colIndex = GetColumn(menu);
                var rowIndex = GetRow(menu);
                if (!IsCellFree(table, colIndex, rowIndex))
                    GetFreeCell(table, out colIndex, out rowIndex);
                // займем ячейку
                table[rowIndex, colIndex] = 1;

                if (menu.HotKey == Key.None)
                {
                    menu.HotKey = GetFreeKey(keyTable);
                    menu.HotKey2 = menu.HotKey;
                }

                Grid.SetColumn(menu, colIndex);
                Grid.SetRow(menu, rowIndex);
                menu.FontSize = FontSize;
                menu.HorizontalContentAlignment = HorizontalAlignment.Left;
                menu.Focusable = false;
                menu.Margin = new Thickness(0, 2, colIndex == maxcolindex ? 0 : 2, 0);
                _grid.Children.Add(menu);
                _lastEndIndex = i;
            }
        }

        private void GetFreeCell(int[,] table, out int column, out int row)
        {
            column = 0;
            row = 0;
            for (var irow = 0; irow < _maxRows; irow++)
            {
                for (var icol = 0; icol < _maxColumns; icol++)
                {
                    if (table[irow, icol] == 0)
                    {
                        row = irow;
                        column = icol;
                        return;
                    }
                }
            }
        }

        private bool IsCellFree(int[,] table, int column, int row)
        {
            if (row >= _maxRows || column >= _maxColumns)
                return false;
            return table[row, column] == 0;
        }

        private Key GetFreeKey(List<Key> keyTable)
        {
            const string keyPrefix = "F";

            for (var i = 1; i <= 12; i++)
            {
                var key = (keyPrefix + i).To(Key.None);
                if (!keyTable.Contains(key))
                {
                    keyTable.Add(key);
                    return key;
                }
            }
            return Key.None;
        }

        //private static void OnCellAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var reference = d as Visual;
        //    if (reference != null)
        //    {
        //        var parent = VisualTreeHelper.GetParent(reference) as FooterMenu;
        //        if (parent != null)
        //            parent.InvalidateMeasure();
        //    }
        //}

        private static bool IsIntValueNotNegative(object value)
        {
            return (int)value >= 0;
        }

        public static void SetColumn(UIElement element, int value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(ColumnProperty, value);
        }

        public static void SetRow(UIElement element, int value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(RowProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static int GetColumn(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (int)element.GetValue(ColumnProperty);
        }

        [AttachedPropertyBrowsableForChildren]
        public static int GetRow(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (int)element.GetValue(RowProperty);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(NavigationDirectionOnGotFocus));
        }
        #endregion . Methods .
    }
}
