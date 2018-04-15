using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views
{
    /// <summary>
    /// Дизайнер RCL.
    /// </summary>
    public partial class RclDesignerView
    {
        private RclDialogLayoutViewModel _model;

        public RclDesignerView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                _model = DataContext as RclDialogLayoutViewModel;
                if (_model == null)
                    return;

                _model.IsWfDesignMode = true;
                RefreshBinding();
            };

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            RestoreLayout();
            //objectDataLayout.IsCustomization = true;
            objectDataLayout.Controller.CustomizationController.SelectionChanged -= OnSelectionChangedCustomizationController;
            objectDataLayout.Controller.CustomizationController.SelectionChanged += OnSelectionChangedCustomizationController;
        }

        private void RestoreLayout()
        {
            if (_model != null)
            {
                objectDataLayout.RestoreLayout(_model.LayoutValue);
                //objectDataLayout.RestoreChildrenLayout();
            }
        }

        private void RefreshBinding()
        {
            if (_model == null)
                return;
            FillGroup(objectDataLayout);
        }

        private void FillGroup(LayoutGroup group)
        {
            if (_model.Source != null)
                group.DataContext = _model.Source;

            Action<Control, ValueDataField> setPropertiesHandler = (control, valueDataField) =>
            {
                if (_model.FontSize > 0)
                    control.FontSize = _model.FontSize;

                control.Tag = valueDataField;

                //т.к. испоьзуем ExpandoObject - регистрируем здесь
                RegisterName(valueDataField.Name, control);
            };

            var layoutGroups = new Dictionary<string, LayoutGroup>();

            foreach (var field in _model.Fields.OrderBy(p => p.Order).ToArray())
            {
                var oldLayout = FindName(field.Name);
                if (oldLayout != null)
                    continue;

                UIElement child = null;
                if (field.FieldType == typeof(FooterMenu) || field.FieldType == typeof(Button) || field.FieldType == typeof(IFooterMenu))
                {
                    var button = new Button
                    {
                        Content = field.Caption,
                        HorizontalContentAlignment = HorizontalAlignment.Left
                    };

                    if (_model.FontSize > 0)
                        button.FontSize = _model.FontSize;

                    var layoutItem = new LayoutItem
                    {
                        Name = field.Name,
                        IsEnabled = !field.IsEnabled.HasValue || field.IsEnabled.Value,
                        Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed,
                        Content = button
                    };
                    setPropertiesHandler(layoutItem, field);
                    child = layoutItem;
                }
                else
                {
                    var layoutItem = new CustomDataLayoutItem(_model.IsWfDesignMode, field)
                    {
                        IsVisibilitySetOutside = true,
                        IsDisplayFormatSetOutside = true
                    };
                    setPropertiesHandler(layoutItem, field);

                    var layoutGroupName = LayoutGroupHelper.GetLayoutGroupNameFromField(field, _model.IsWfDesignMode);
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
           SaveLayout(false);
           DialogResult = true;
           Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnClickClearChildrenLayout(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(string.Format("Вы уверены, что хотите очистить настройки диалога дочерних элементов?{0}Настройки будут сброшены, форма закрыта. Продолжить?", Environment.NewLine), Title, MessageBoxButton.OKCancel, MessageBoxImage.Question,
                    MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                SaveLayout(true);
                DialogResult = true;
                Close();
            }
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

        private void SaveLayout(bool doNotSaveChildrenLayout)
        {
            var doNotSaveChildrenLayoutInternal = objectDataLayout.DoNotUseChildrenLayout;
            try
            {
                objectDataLayout.DoNotUseChildrenLayout = doNotSaveChildrenLayout;
                if (_model != null)
                    _model.LayoutValue = objectDataLayout.SaveLayout();
            }
            finally
            {
                objectDataLayout.DoNotUseChildrenLayout = doNotSaveChildrenLayoutInternal;
            }
        }

        private void OnDesignerButtonClick(object sender, RoutedEventArgs e)
        {
            if (_model == null || _model.SelectedElement == null)
                return;

            var layoutItem = _model.SelectedElement as LayoutItem;
            if (layoutItem == null || layoutItem.Content == null)
                return;

            var customComboBoxEditRcl = layoutItem.Content as CustomComboBoxEditRcl;
            if (customComboBoxEditRcl == null)
                return;

            customComboBoxEditRcl.IsWfDesignMode = true;
            customComboBoxEditRcl.OpenPopup();
        }
    }

    public sealed class FieldToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is ValueDataField))
                return Visibility.Collapsed;

            var field = (ValueDataField) value;
            var lookupType = RclLookupType.None;
            string svalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.LookupType, true, out svalue))
                lookupType = svalue.To(RclLookupType.None);

            return lookupType == RclLookupType.DefaultGrid ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
