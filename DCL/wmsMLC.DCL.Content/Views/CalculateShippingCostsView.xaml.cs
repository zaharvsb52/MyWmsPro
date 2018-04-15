using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.DCL.Main.Views;

namespace wmsMLC.DCL.Content.Views
{
    public partial class CalculateShippingCostsView
    {
        //Also, if you want to customize or remove the row indicator icon, please refer to the How to provide any text for the RowIndicator Code Central example to see how to override the mentioned RowIndicator template.
        //https://www.devexpress.com/Support/Center/Example/Details/E2362
        //https://www.devexpress.com/Support/Center/Question/Details/T337065
        //How to change the row error icon which is displayed within a row indicator
        //https://www.devexpress.com/Support/Center/Question/Details/A2841

        public CalculateShippingCostsView()
        {
            InitializeComponent();
            var columnTemplateSelector = ObjectListGridControl.ColumnGeneratorTemplateSelector as Main.Helpers.ColumnTemplateSelector;
            if (columnTemplateSelector != null)
                columnTemplateSelector.AllowUseLookUpEdit = true;
            DataContextChanged += OnDataContextChanged;
            ObjectListGridControl.Loaded += OnGridControlLoaded;
            ObjectListGridControl.RestoredLayoutFromXml += OnGridControlLayoutFromXml;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldNpch = e.OldValue as INotifyPropertyChanged;
            if (oldNpch != null)
                oldNpch.PropertyChanged -= DataContextPropertyChanged;

            SubscribeOnRefreshView();
            var vm = DataContext as IObjectViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }
            RefreshBinding();

            var newNpch = e.NewValue as INotifyPropertyChanged;
            if (newNpch != null)
                newNpch.PropertyChanged += DataContextPropertyChanged;

            var mmv = DataContext as CalculateShippingCostsViewModel;
            if (mmv != null)
            {
                mmv.NewRowAdded -= OnNewRowAdded;
                mmv.NewRowAdded += OnNewRowAdded;
            }
        }

        private void OnNewRowAdded(object sender, EventArgs eventArgs)
        {
            EndEdit();
            ObjectListGridControl.CurrentColumn = ObjectListGridControl.Columns[0];
            var index = DataControlBase.NewItemRowHandle;
            ObjectListGridControl.View.MoveFocusedRow(index);
            ObjectListGridControl.View.FocusedRowHandle = index;
            ObjectListGridControl.View.ShowEditor();
        }

        private void OnGridControlLoaded(object sender, RoutedEventArgs e)
        {
            ObjectListGridControl.Loaded -= OnGridControlLoaded;
            RestoreColumnsSettings();
        }

        private void OnGridControlLayoutFromXml(object sender, EventArgs e)
        {
            ObjectListGridControl.RestoredLayoutFromXml -= OnGridControlLayoutFromXml;
            RestoreColumnsSettings();
        }

        private void RestoreColumnsSettings()
        {
            var settings = ObjectListGridControl.Columns.Where(p => p.EditSettings is CustomLookUpEditSettings &&
                !((CustomLookUpEditSettings)p.EditSettings).NotLoadDataFromEditor)
                    .Select(p => (CustomLookUpEditSettings)p.EditSettings)
                    .ToArray();
            foreach (var s in settings)
            {
                s.NotLoadDataFromEditor = true;
            }
        }

        private void DataContextPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == SourceViewModelBase<int>.SourcePropertyName)
                RefreshBinding();
        }

        private void RefreshBinding()
        {
            var model = DataContext as ICustomModelHandler;
            if (model != null)
                FillGroup(objectDataLayout, model);
        }

        private void RefreshViewEvent(object sender, EventArgs e)
        {
            RefreshBinding();
        }

        private void SubscribeOnRefreshView()
        {
            var vm = DataContext as IModelHandler;
            if (vm == null)
                return;

            UnSubscribeOnRefreshView();
            vm.RefreshViewEvent += RefreshViewEvent;
        }

        private void UnSubscribeOnRefreshView()
        {
            var vm = DataContext as IModelHandler;
            if (vm != null)
                vm.RefreshViewEvent -= RefreshViewEvent;
        }

        private void FillGroup(LayoutGroup group, ICustomModelHandler vm)
        {
            var inPropertyEditMode = false;
            var isOldLayout = false;
            if (vm.Source != null)
                group.DataContext = vm.Source;
            var pe = vm as IPropertyEditHandler;
            if (pe != null)
                inPropertyEditMode = pe.InPropertyEditMode;
            foreach (var field in vm.Fields.OrderBy(p => p.Order).ToArray())
            {
                bool? isMergedProperty = null;
                if (inPropertyEditMode)
                    isMergedProperty = pe.IsMergedPropery(field.Name);

                var oldLayout = FindName(field.Name);
                if ((isOldLayout = oldLayout != null) && !field.IsChangeLookupCode)
                {
                    var li = oldLayout as CustomDataLayoutItem;
                    if (li != null)
                    {
                        li.SetLabelProperties(field);
                        li.Visibility = field.Visible ? Visibility.Visible : Visibility.Hidden;
                        if (field.IsEnabled.HasValue)
                            li.IsReadOnly = !field.IsEnabled.Value;

                        if (field.SetFocus)
                        {
                            li.SetFocus = field.SetFocus;
                        }

                        li.Binding = new Binding(string.IsNullOrEmpty(field.FieldName) ? field.Name : field.FieldName)
                        {
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            ValidatesOnDataErrors = true,
                        };
                    }
                    else
                    {
                        var layItem = oldLayout as LayoutItem;
                        if (layItem != null)
                        {
                            layItem.Visibility = field.Visible ? Visibility.Visible : Visibility.Hidden;
                            if (field.IsEnabled.HasValue)
                            {
                                layItem.IsEnabled = field.IsEnabled.Value;
                                layItem.Content.IsEnabled = field.IsEnabled.Value;

                            }
                        }
                    }
                    continue;
                }

                var isChangeName = false;
                if (Regex.IsMatch(field.Name, @"^[0-9]"))
                {
                    field.Name = "_" + field.Name;
                    isChangeName = true;
                }

                LayoutItem layoutItem;
                LayoutGroup foundGroup = null;
                var index = 0;

                if (oldLayout != null)
                {
                    foundGroup = ObjectViewBase.SearchGroup(group, (UIElement)oldLayout);
                    if (foundGroup == null)
                        continue;
                    index = foundGroup.Children.IndexOf((UIElement)oldLayout);
                    if (index < 0)
                        continue;
                    foundGroup.Children.Remove((UIElement)oldLayout);
                    UnregisterName(((FrameworkElement)oldLayout).Name);
                }

                if (typeof(Button).IsAssignableFrom(field.FieldType))
                {
                    Key currKey;
                    layoutItem = new LayoutItem()
                    {
                        Name = field.Name,
                        IsEnabled = !field.IsEnabled.HasValue || field.IsEnabled.Value,
                        Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed,

                        Content = new CustomButton
                        {
                            Content = field.Caption,
                            Command = vm.MenuCommand,

                            HotKey = Enum.TryParse(field.HotKey, out currKey) ? currKey : Key.None,
                            CommandParameter = new KeyValuePair<string, object>(field.Name, field.Value),
                            Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed,
                            IsEnabled = !field.IsEnabled.HasValue || field.IsEnabled.Value
                        }
                    };
                }
                else
                {
                    layoutItem = new CustomDataLayoutItem(field)
                    {
                        IsVisibilitySetOutside = true,
                        IsDisplayFormatSetOutside = true,
                        IsLabelFontWeightBold = field.IsLabelFontWeightBold,
                        ParentViewModelSource = (vm is ICPV) ? vm.ParentViewModelSource : null,
                        LookupButtonEnabled = field.LookupButtonEnabled,
                        ToolTipIns = CreateCustomSuperToolTip(field),
                        IsMergedProperty = isMergedProperty
                    };
                }

                //т.к. испоьзуем ExpandoObject - регистрируем здесь
                RegisterName(field.Name, layoutItem);
                if (isChangeName)
                    field.Name = field.Name.Substring(1);

                if (oldLayout != null && index > -1)
                    foundGroup.Children.Insert(index, layoutItem);
                else group.Children.Add(layoutItem);
            }

            if (!isOldLayout)
                objectDataLayout.RestoreLayout(vm.LayoutValue);
            if (vm.InsertFromAvailableItems && objectDataLayout.AvailableItems.Count > 0)
            {
                foreach (var p in objectDataLayout.AvailableItems.ToArray())
                {
                    objectDataLayout.AvailableItems.Remove(p);
                    objectDataLayout.Children.Add(p);
                }
            }
        }

        public StackPanel CreateCustomSuperToolTip(DataField field)
        {
            var stack = new StackPanel();
            stack.Children.Add(new TextBlock { Text = field.Caption });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = field.Description });
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock { Text = string.Format("[{0}]", field.Name), FontFamily = new FontFamily("Segoe UI"), Foreground = Brushes.Gray, FontSize = 11 });
            return stack;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var child in objectDataLayout.Children)
                {
                    var disposable = child as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            // прибиваем Layout
            if (objectDataLayout != null)
                objectDataLayout.Dispose();
            base.Dispose(disposing);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            foreach (var p in VisualTreeHelperExt.FindChildsByType<CustomButton>(this).Where(p => p.IsHotKey(e.Key)))
            {
                p.PreviewHotKey(e);
            }
            base.OnPreviewKeyDown(e);
        }

        private void OnGridControlItemClick(object sender, ItemClickEventArgs e)
        {
            var oldMode = ObjectListGridControl.ClipboardCopyMode;
            try
            {
                var mode = (ClipboardCopyMode)Enum.Parse(typeof(ClipboardCopyMode), e.Item.CommandParameter.ToString());
                ObjectListGridControl.ClipboardCopyMode = mode;
                ObjectListGridControl.ByCheckCopyMode = false;
                ObjectListGridControl.CopyToClipboard();
            }
            finally
            {
                ObjectListGridControl.ClipboardCopyMode = oldMode;
                ObjectListGridControl.ByCheckCopyMode = true;
            }
        }
        private void EndEdit()
        {
            ObjectListGridControl.View.CommitEditing();
        }
    }
}
