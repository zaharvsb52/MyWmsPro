using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views
{
    public partial class DialogLayoutView
    {
        private DialogLayoutViewModel _model;

        public DialogLayoutView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
                {
                    _model = DataContext as DialogLayoutViewModel;
                    if (_model == null)
                        return;

                    RefreshBinding();
                };
            Loaded += delegate
                {
                    RestoreLayout();
                    objectDataLayout.IsCustomization = true;
                    objectDataLayout.Controller.CustomizationController.SelectionChanged -= OnSelectionChangedCustomizationController;
                    objectDataLayout.Controller.CustomizationController.SelectionChanged += OnSelectionChangedCustomizationController;
                };
        }

        private void RestoreLayout()
        {
            if (_model != null)
                objectDataLayout.RestoreLayout(_model.LayoutValue);
        }

        private void RefreshBinding()
        {
            if (_model == null)
                return;
            FillGroup(objectDataLayout, _model);
        }

        private void FillGroup(LayoutGroup group, DialogLayoutViewModel vm)
        {
            if (vm.Source != null)
                group.DataContext = vm.Source;

            foreach (var field in vm.Fields.OrderBy(p => p.Order).ToArray())
            {
                var oldLayout = FindName(field.Name);
                if (oldLayout != null)
                    continue;

                LayoutItem layoutItem;
                if (field.FieldType == typeof(Button) || field.FieldType == typeof(IFooterMenu))
                {
                    var button = new Button
                    {
                        Content = field.Caption,
                        HorizontalContentAlignment = HorizontalAlignment.Left
                    };

                    if (vm.FontSize > 0)
                        button.FontSize = vm.FontSize;

                    layoutItem = new LayoutItem
                    {
                        Name = field.Name,
                        IsEnabled = !field.IsEnabled.HasValue || field.IsEnabled.Value,
                        Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed,
                        Content = button
                    };
                }
                else
                {
                    layoutItem = new CustomDataLayoutItem(field)
                    {
                        IsVisibilitySetOutside = true,
                        IsDisplayFormatSetOutside = true,
                        //ParentViewModelSource = vm.ParentViewModelSource,
                    };
                }

                if (vm.FontSize > 0)
                    layoutItem.FontSize = vm.FontSize;

                layoutItem.Tag = field;

                //т.к. испоьзуем ExpandoObject - регистрируем здесь
                RegisterName(field.Name, layoutItem);
                group.Children.Add(layoutItem);
            }
        }

        private void OnSelectionChangedCustomizationController(object sender, LayoutControlSelectionChangedEventArgs e)
        {
            if (e.SelectedElements.Count == 1 && !ReferenceEquals(e.SelectedElements[0], objectDataLayout))
            {
                if (_model != null)
                    _model.SelectedElement = e.SelectedElements[0];
                propGrid.SelectedObject = e.SelectedElements[0].Tag;
            }
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            if (_model != null)
                _model.LayoutValue = objectDataLayout.SaveLayout();
            DialogResult = true;
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnClickClearLayout(object sender, RoutedEventArgs e)
        {
            if (_model == null)
                return;
            if (MessageBox.Show(string.Format("Вы уверены, что хотите очистить настройки диалога?{0}Настройки будут сброшены, форма закрыта. Продолжить?", Environment.NewLine), Title, MessageBoxButton.OKCancel, MessageBoxImage.Question,
                    MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                _model.LayoutValue = null;
                DialogResult = true;
                Close();
            }
        }
    }
}
