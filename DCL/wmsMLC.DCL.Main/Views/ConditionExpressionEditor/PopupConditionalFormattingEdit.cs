using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Xpf.Core.ConditionalFormatting;
using DevExpress.Xpf.Core.ConditionalFormatting.Themes;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Commands;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    public class PopupConditionalFormattingEdit : PopupBaseEdit
    {
        #region . Properties .

        public TableView View
        {
            get { return (TableView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View", typeof(TableView), typeof(PopupConditionalFormattingEdit), new PropertyMetadata(OnViewChanged));

        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupConditionalFormattingEdit)d).SetEnabled();
        }

        public ConditionalFormattingThemeKeys TemplateKey
        {
            get { return (ConditionalFormattingThemeKeys)GetValue(TemplateKeyProperty); }
            set { SetValue(TemplateKeyProperty, value); }
        }

        public static readonly DependencyProperty TemplateKeyProperty = DependencyProperty.Register("TemplateKey", typeof(ConditionalFormattingThemeKeys), typeof(PopupConditionalFormattingEdit));

        public string FieldName
        {
            get { return (string)GetValue(FieldNameProperty); }
            set { SetValue(FieldNameProperty, value); }
        }

        public static readonly DependencyProperty FieldNameProperty = DependencyProperty.Register("FieldName", typeof(string), typeof(PopupConditionalFormattingEdit), new PropertyMetadata(OnFieldNameChanged));

        private static void OnFieldNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupConditionalFormattingEdit)d).SetEnabled();
        }

        public ICommand ConditionalFormattingEditCommand { get; private set; }
        #endregion . Properties .

        #region . Methods .

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (ConditionalFormattingEditCommand == null)
                ConditionalFormattingEditCommand = new DelegateCustomCommand<object>(OnConditionalFormattingEdit);

            SetEnabled();
        }

        private void SetEnabled()
        {
            IsEnabled = View != null && !string.IsNullOrEmpty(FieldName);
        }

        protected override void OnPopupOpened()
        {
            var content = CreateConditionFormatContent();
            if (content != null)
                Popup.PopupContent = content;
            base.OnPopupOpened();
        }

        private void OnConditionalFormattingEdit(object parameter)
        {
            if (parameter == null)
                return;

            ClosePopup();
            var condition = parameter as FormatConditionBase;
            if (condition == null)
            {
                EditValue = null;
                return;
            }

            EditValue = condition.PredefinedFormatName;
        }

        private FrameworkElement CreateConditionFormatContent()
        {
            SetEnabled();
            if (!IsEnabled)
                return null;

            var content = TemplateHelper.LoadFromTemplate<FrameworkElement>((DataTemplate)View.FindResource(new ConditionalFormattingThemeKeyExtension { ResourceKey = TemplateKey }));

            IEnumerable groups;
            switch (TemplateKey)
            {
                case ConditionalFormattingThemeKeys.ColorScaleMenuItemContent:
                    groups = GetGroupedFormatItems<DataBarFormatCondition>(View.PredefinedColorScaleFormats, x => new[] { x.Icon });
                    break;
                case ConditionalFormattingThemeKeys.DataBarMenuItemContent:
                    groups = GetGroupedFormatItems<DataBarFormatCondition>(View.PredefinedDataBarFormats, x => new[] { x.Icon });
                    break;
                case ConditionalFormattingThemeKeys.IconSetMenuItemContent:
                    groups = GetGroupedFormatItems<DataBarFormatCondition>(View.PredefinedIconSetFormats,
                        x => ((IconSetFormat) x.Format).Elements.Select(y => y.Icon));
                    break;
                default:
                    throw new DeveloperException("Undefined FormatConditionType '{0}'.", TemplateKey);
            }

            //content.DataContext = new GridColumnMenuInfo.FormatsViewModel(groups);
            content.DataContext = new FormatsViewModel(groups);
            return content;
        }

        private IEnumerable GetGroupedFormatItems<TFormatCondition>(IEnumerable<FormatInfo> formatInfo, Func<FormatInfo, IEnumerable<ImageSource>> iconsExtractor) where TFormatCondition : IndicatorFormatConditionBase, new()
        {
            SetEnabled();
            if (!IsEnabled)
                return null;

            return formatInfo
                    .GroupBy(x => x.GroupName)
                    .Select(x => new
                    {
                        Header = x.Key,
                        Items = ConvertToBindableItems<TFormatCondition>(x, iconsExtractor),
                    }).ToArray();
        }

        private IEnumerable ConvertToBindableItems<TFormatCondition>(IEnumerable<FormatInfo> formatInfo, Func<FormatInfo, IEnumerable<ImageSource>> iconsExtractor) where TFormatCondition : IndicatorFormatConditionBase, new()
        {
            return formatInfo.Select(x => new
            {
                Name = x.DisplayName, 
                x.Description,
                Icons = iconsExtractor(x).ToArray(),
                //Command = view.TableViewCommands.AddFormatCondition,
                Command = ConditionalFormattingEditCommand,
                x.Format,
                FormatCondition = new TFormatCondition { PredefinedFormatName = x.FormatName, FieldName = FieldName }
            }).ToArray();
        }

        #endregion . Methods .

        private class FormatsViewModel
        {
            public FormatsViewModel(IEnumerable groups)
            {
                FormatConditionGroups = groups;
            }

            public IEnumerable FormatConditionGroups { get; private set; }
        }
    }
}
