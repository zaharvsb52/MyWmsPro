using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.Content.Views
{
    public partial class ProductChangeReservedView
    {
        public ProductChangeReservedView()
        {
            InitializeComponent();

            LayoutRoot.Children.Remove(spinEdit);
            DataContextChanged += ObjectView_DataContextChanged;
        }

        private void ObjectView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldNpch = e.OldValue as INotifyPropertyChanged;
            if (oldNpch != null)
                oldNpch.PropertyChanged -= DataContextPropertyChanged;
            var vm = DataContext as IObjectViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }
            RefreshBinding();

            var newNpch = e.NewValue as INotifyPropertyChanged;
            if (newNpch != null)
                newNpch.PropertyChanged += DataContextPropertyChanged;
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

        private void FillGroup(LayoutGroup group, ICustomModelHandler vm)
        {
            //TODO: по хорошему нужно писать свой контрол на остнове DataLayout-а (научить правильно биндиться, понимать аттрибуты и т.д.)
            if (vm.Source != null)
                group.DataContext = vm.Source;

            //HACK: всегда берем последний элемент (с макс. order'ом) для SpinEdit'а
            var maxorder = vm.Fields.Max(p => p.Order);
            foreach (var field in vm.Fields.OrderBy(p => p.Order).ToArray())
            {
                var oldLayout = FindName(field.Name);
                if (oldLayout != null)
                    continue;

                var isChangeName = false;
                if (Regex.IsMatch(field.Name, @"^[0-9]"))
                {
                    field.Name = "_" + field.Name;
                    isChangeName = true;
                }

                LayoutItem layoutItem;
                if (maxorder > 0 && field.Order == maxorder)
                {
                    layoutItem = new LayoutItem
                    {
                        Name = field.Name,
                        LabelPosition = field.LabelPosition.To(LayoutItemLabelPosition.Left),
                        Label = new Label {Content = field.Caption},
                        IsEnabled = !field.IsEnabled.HasValue || field.IsEnabled.Value,
                        Visibility = field.Visible ? Visibility.Visible : Visibility.Hidden,
                        Content = spinEdit
                    };
                }
                else
                {
                    layoutItem = new CustomDataLayoutItem(field)
                    {
                        IsVisibilitySetOutside = true,
                        IsDisplayFormatSetOutside = true,
                        IsLabelFontWeightBold = field.IsLabelFontWeightBold,
                        ParentViewModelSource = vm.Source,
                        ToolTipIns = CreateCustomSuperToolTip(field),
                    };
                    ((CustomDataLayoutItem) layoutItem).SetLabelProperties(field);
                }

                //т.к. испоьзуем ExpandoObject - регистрируем здесь
                RegisterName(field.Name, layoutItem);
                if (isChangeName)
                    field.Name = field.Name.Substring(1);
                group.Children.Add(layoutItem);
            }

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
            stack.Children.Add(new TextBlock {Text = field.Caption});
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock {Text = field.Description});
            stack.Children.Add(new SuperTipItemControlSeparator());
            stack.Children.Add(new TextBlock
            {
                Text = string.Format("[{0}]", field.Name),
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = Brushes.Gray,
                FontSize = 11
            });
            return stack;
        }
    }
}
