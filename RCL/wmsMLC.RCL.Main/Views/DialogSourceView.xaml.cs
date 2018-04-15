using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.RCL.Main.Views
{
    public partial class DialogSourceView
    {
        public DialogSourceView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            KeyHelper.ViewPreviewKeyDown(this, e);
            base.OnPreviewKeyDown(e);
            //Gleb для терминала
            //MessageBox.Show(string.Format("{0}", e.Key));
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldNpch = e.OldValue as INotifyPropertyChanged;
            if (oldNpch != null)
                oldNpch.PropertyChanged -= OnModelPropertyChanged;

            RefreshBinding();

            var newNpch = e.NewValue as INotifyPropertyChanged;
            if (newNpch != null)
                newNpch.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as DialogSourceViewModel;
            if (model == null)
                return;
            if (e.PropertyName == model.GetType().ExtractPropertyName(() => model.Source))
                RefreshBinding(model);
        }

        private void RefreshBinding()
        {
            RefreshBinding(DataContext as DialogSourceViewModel);
        }

        private void RefreshBinding(DialogSourceViewModel model)
        {
            if (model == null)
                return;
            FillGroup(objectDataLayout, model);
        }

        private void FillGroup(LayoutGroup group, DialogSourceViewModel vm)
        {
            //TODO: по хорошему нужно писать свой контрол на остнове DataLayout-а (научить правильно биндиться, понимать аттрибуты и т.д.)
            if (vm.Source != null)
                group.DataContext = vm.Source;
            group.Children.Clear();
            var setfocus = false;

            var layoutGroups = new Dictionary<string, LayoutGroup>();

            foreach (var field in vm.Fields.OrderBy(p => p.Order).ToArray())
            {
                var oldLayout = FindName(field.Name);
                if (oldLayout != null)
                    continue;
                if (field.SetFocus)
                    setfocus = true;

                UIElement child = null;
                if (field.FieldType == typeof(Button) || field.FieldType == typeof(CustomButton))
                {
                    var layoutItem = new LayoutItem
                    {
                        Name = field.Name,
                        IsEnabled = field.IsEnabled.HasValue && field.IsEnabled.Value,
                        Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed,
                        Content = MenuHelper.CreateCustomButton(field, vm.MenuCommand, vm.FontSize, false)
                    };
                    RegisterName(field.Name, layoutItem);
                    child = layoutItem;
                }
                else if (field.FieldType == typeof(IFooterMenu) || field.FieldType == typeof(FooterMenu))
                {
                    MenuHelper.CreateFooterMenu(footerMenuControl, field, vm.MenuCommand, vm.FontSize, false);
                    continue;
                }
                else
                {
                    var layoutItem = new CustomDataLayoutItem(vm.IsWfDesignMode, field)
                    {
                        IsVisibilitySetOutside = true,
                        IsDisplayFormatSetOutside = true,

                        FontSize = vm.FontSize,

                        IsReadOnlySetOutside = true,
                        TooltipDisable = true,
                        CloseDialogCommand = vm.MenuCommand
                    };
                    RegisterName(field.Name, layoutItem);

                    var layoutGroupName = LayoutGroupHelper.GetLayoutGroupNameFromField(field, vm.IsWfDesignMode);
                    if (string.IsNullOrEmpty(layoutGroupName))
                    {
                        child = layoutItem;
                    }
                    else
                    {
                        LayoutGroup layoutGroup;
                        if (!layoutGroups.ContainsKey(layoutGroupName))
                        {
                            layoutGroup = LayoutGroupHelper.CreateLayoutGroup(layoutGroupName);
                            layoutGroups[layoutGroupName] = layoutGroup;
                            RegisterName(layoutGroupName, layoutGroup);
                            group.Children.Add(layoutGroup);
                        }

                        layoutGroup = layoutGroups[layoutGroupName];
                        layoutGroup.Children.Add(layoutItem);
                    }
                }

                if (child != null)
                    group.Children.Add(child);
            }

            if (!setfocus)
                KeyHelper.SetFocusElement(group.Children);

            objectDataLayout.RestoreLayout(vm.LayoutValue);
        }
    }
}
