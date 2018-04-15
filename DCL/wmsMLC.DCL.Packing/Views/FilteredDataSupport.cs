using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.Xpf.Grid;

namespace wmsMLC.DCL.Packing.Views
{
    public class FilteredDataSupport
    {
        public static readonly DependencyProperty VisibleDataProperty =
            DependencyProperty.RegisterAttached("VisibleData", typeof(IList), typeof(FilteredDataSupport), new PropertyMetadata(OnDataChanged));

        public static void SetVisibleData(UIElement element, IList value)
        {
            element.SetValue(VisibleDataProperty, value);
        }
        public static IList GetVisibleData(UIElement element)
        {
            return (IList)element.GetValue(VisibleDataProperty);
        }

        private static void OnCustomRowFilter(object sender, RowFilterEventArgs e)
        {
            if (e.ListSourceRowIndex != 0)
                return;
            ChangeVisibleData(sender as GridControl);
        }

        private static void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null)
                return;
            if (grid.IsFilterEnabled)
                ChangeVisibleData(grid);
        }

        static void ChangeVisibleData(GridControl grid)
        {
            var visibleData = grid.GetValue(VisibleDataProperty) as IList;
            if (visibleData == null)
                return;
            var excludedData = grid.GetValue(ExcludedDataProperty) as IList;
            var data = grid.ItemsSource as IList;
            if (data == null)
                return;

            var itemType = GetItemType(data);
            if (itemType == null)
                return;

            visibleData.Clear();
            if (grid.IsFilterEnabled)
            {
                var evaluator = new ExpressionEvaluator(TypeDescriptor.GetProperties(itemType), grid.FilterCriteria, false);
                var filteredCollection = evaluator.Filter(data);

                foreach (object item in filteredCollection)
                    if (excludedData == null || !excludedData.Contains(item))
                        visibleData.Add(item);
            }
            else
            {
                foreach (var item in data)
                    if (excludedData == null || !excludedData.Contains(item))
                        visibleData.Add(item);
            }
        }

        private static Type GetItemType(IEnumerable data)
        {
            foreach (var item in data)
                return item.GetType();
            return null;
        }

        public static readonly DependencyProperty ExcludedDataProperty =
            DependencyProperty.RegisterAttached("ExcludedData", typeof(IList), typeof(FilteredDataSupport), new PropertyMetadata(null, OnDataChanged, OnCoerceExcludedData));

        public static void SetExcludedData(UIElement element, IList value)
        {
            element.SetValue(ExcludedDataProperty, value);
        }
        public static IList GetExcludedData(UIElement element)
        {
            return (IList)element.GetValue(ExcludedDataProperty);
        }

        private static void OnDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null)
                return;
            if (e.OldValue == null && e.NewValue != null)
            {
                if (e.Property == ExcludedDataProperty)
                    grid.CustomRowFilter += OnExcludeCustomRowFilter;
                else if (e.Property == VisibleDataProperty)
                {
                    grid.CustomRowFilter += OnCustomRowFilter;
                    grid.FilterChanged += OnFilterChanged;
                }
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                if (e.Property == ExcludedDataProperty)
                    grid.CustomRowFilter -= OnExcludeCustomRowFilter;
                else if (e.Property == VisibleDataProperty)
                {
                    grid.CustomRowFilter -= OnCustomRowFilter;
                    grid.FilterChanged -= OnFilterChanged;
                }
            }
            grid.RefreshData();
        }

        private static object OnCoerceExcludedData(DependencyObject sender, object data)
        {
            var grid = sender as GridControl;
            if (grid == null)
                return data; 
            grid.RefreshData();
            return data;
        }

        private static void OnExcludeCustomRowFilter(object sender, RowFilterEventArgs e)
        {
            var grid = sender as GridControl;
            if (grid == null)
                return;
            var data = grid.ItemsSource as IList;
            if (data == null)
                return;
            var excludedData = grid.GetValue(ExcludedDataProperty) as IList;
            if (excludedData == null)
                return;
            e.Visible = !excludedData.Contains(data[e.ListSourceRowIndex]);
            e.Handled = !e.Visible;
        }
    }
}