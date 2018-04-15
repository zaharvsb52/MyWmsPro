using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Grid;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    public class StyleOption : ExpressionStyleOption, IDataErrorInfo, ICloneable
    {
        public StyleOption()
        {
        }

        public StyleOption(ExpressionStyleOption expressionStyleOption): this()
        {
            Clone(expressionStyleOption, this);
        }

        /// <summary>
        /// Свойство предназначено для использовании в xaml'е. Не сохраняется в файле настроек, если значение - true.
        /// </summary>
        public bool IsReadOnly { get; set; }

        //public DataTrigger StyleTrigger { get; set; }
        public MultiDataTrigger StyleTrigger { get; set; }
        public StyleOptionCollection Parent { get; internal set; }

        public string Error
        {
            get { return null; }
        }

        public string this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case NamePropertyName:
                        if (Name.IsNullOrEmptyAfterTrim())
                            return Resources.StringResources.StyleOptionNameNotDefined;

                        var error = ValidateFormatConditionType();
                        if (!string.IsNullOrEmpty(error))
                            return error;

                        error = ValidateExpressionString();
                        if (!string.IsNullOrEmpty(error))
                            return error;

                        error = ValidateThreshold();
                        if (!string.IsNullOrEmpty(error))
                            return error;

                        error = ValidateFieldName();
                        if (!string.IsNullOrEmpty(error))
                            return error;

                        return ValidatePredefinedFormatName();
                    case ExpressionStringPropertyName:
                        return ValidateExpressionString();
                    case FieldNamePropertyName:
                        return ValidateFieldName();
                    case ThresholdPropertyName:
                        return ValidateThreshold();
                    case FormatConditionTypePropertyName:
                        return ValidateFormatConditionType();
                    case PredefinedFormatNamePropertyName:
                        return ValidatePredefinedFormatName();
                }
                return null;
            }
        }

        private string ValidateExpressionString()
        {
            if (FormatConditionType != FormatConditionType.Default)
                return null;

            if (ExpressionString.IsNullOrEmptyAfterTrim())
                return Resources.StringResources.StyleOptionConditionNotDefined;

            return Parent.ValidateExpression(ExpressionString);
        }

        private string ValidateFieldName()
        {
            if (ApplyToRow && FormatConditionType == FormatConditionType.Default)
                return null;
            if (FieldName.IsNullOrEmptyAfterTrim())
                return Resources.StringResources.StyleOptionFieldNameError;
            return null;
        }

        private string ValidateThreshold()
        {
            switch (FormatConditionType)
            {
                case FormatConditionType.TopItemsRule:
                case FormatConditionType.TopPersentRule:
                case FormatConditionType.BottomItemsRule:
                case FormatConditionType.BottomPercentRule:
                    if (Threshold <= 0)
                    {
                        var fieldname = (FormatConditionType == FormatConditionType.TopItemsRule || FormatConditionType == FormatConditionType.BottomItemsRule)
                            ? Resources.StringResources.ConditionalFormattingWindowAppearanceGroupThresholdLabel
                            : Resources.StringResources.ConditionalFormattingWindowAppearanceGroupThresholdInPercentLabel;
                        return string.Format(Resources.StringResources.StyleOptionThresholdError, fieldname);
                    }
                    break;
            }

            return null;
        }

        private string ValidateFormatConditionType()
        {
            switch (FormatConditionType)
            {
                case FormatConditionType.AboveAverageRule:
                case FormatConditionType.BelowAverageRule:
                case FormatConditionType.ColorScale:
                case FormatConditionType.DataBar:
                case FormatConditionType.IconSet:
                    if (string.IsNullOrEmpty(FieldName) || Parent.ColumnInfo == null ||
                        Parent.ColumnInfo.Columns == null)
                        return Resources.StringResources.StyleOptionFormatConditionTypeColumnError;
                    var columnType =
                        Parent.ColumnInfo.Columns.Where(p => p.FieldName == FieldName)
                            .Select(p => p.FieldType)
                            .SingleOrDefault();
                    if (columnType == null)
                        return Resources.StringResources.StyleOptionFormatConditionTypeColumnError;

                    if (!columnType.GetNonNullableType().IsNumeric())
                        return Resources.StringResources.StyleOptionFormatConditionTypeColumnTypeError;
                    break;
            }

            return null;
        }

        private string ValidatePredefinedFormatName()
        {
            switch (FormatConditionType)
            {
                case FormatConditionType.ColorScale:
                case FormatConditionType.DataBar:
                case FormatConditionType.IconSet:
                    if (PredefinedFormatName.IsNullOrEmptyAfterTrim())
                        return string.Format(Resources.StringResources.StyleOptionPredefinedFormatNameError, Resources.StringResources.ConditionalFormattingWindowAppearanceGroupFormatLabel);
                    break;
            }

            return null;   
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case FieldNamePropertyName:
                    base.OnPropertyChanged(FormatConditionTypePropertyName);
                    base.OnPropertyChanged(PredefinedFormatNamePropertyName);
                    break;
                case ApplyToRowPropertyName:
                    base.OnPropertyChanged(FieldNamePropertyName);
                    break;
            }

            if (propertyName != NamePropertyName)
                base.OnPropertyChanged(NamePropertyName);
        }

        public string[] Validate()
        {
            var result = new List<string>();
            var pdsk = TypeDescriptor.GetProperties(this);
            foreach (PropertyDescriptor p in pdsk)
            {
                base.OnPropertyChanged(p.Name);
                var message = this[p.Name];
                if (!string.IsNullOrEmpty(message))
                    result.Add(message);
            }
            return result.ToArray();
        }

        public void GenerateSetters()
        {
            var triggerBinding = new Binding
            {
                Converter = new ExpressionConverter(),
                ConverterParameter = Parent.ConvertToFields(ExpressionString),
                Path = ApplyToRow ? new PropertyPath("Row") : new PropertyPath("RowData.Row")
            };
            //StyleTrigger = new DataTrigger { Value = true, Binding = triggerBinding };
            StyleTrigger = new MultiDataTrigger();
            StyleTrigger.Conditions.Add(new Condition {Binding = triggerBinding, Value = true});
            StyleTrigger.Conditions.Add(new Condition
            {
                Binding = new Binding("SelectionState"),
                Value = SelectionState.None
            });


            if (Foreground != null)
                StyleTrigger.Setters.Add(new Setter(Control.ForegroundProperty, new Binding("Foreground") { Source = this }));
            if (Background != null)
            {
                const string backgroundPropertyName = "Background";
                StyleTrigger.Setters.Add(new Setter(Control.BackgroundProperty, new Binding(backgroundPropertyName) { Source = this }));
                StyleTrigger.Setters.Add(new Setter(LightweightCellEditor.BackgroundProperty, new Binding(backgroundPropertyName) { Source = this }));
            }
            if (FontFamily != null)
                StyleTrigger.Setters.Add(new Setter(Control.FontFamilyProperty, new Binding("FontFamily") { Source = this }));
            if (FontStyle.HasValue)
                StyleTrigger.Setters.Add(new Setter(Control.FontStyleProperty, new Binding("FontStyle") { Source = this }));
            if (FontSize > 0)
                StyleTrigger.Setters.Add(new Setter(Control.FontSizeProperty, new Binding("FontSize") { Source = this }));
            if (FontWeight.HasValue)
                StyleTrigger.Setters.Add(new Setter(Control.FontWeightProperty, new Binding("FontWeight") { Source = this }));
        }

        public StyleOption Clone()
        {
            var dest = new StyleOption();
            Clone(this, dest);
            dest.IsReadOnly = IsReadOnly;
            return dest;
        }

        private static void Clone(ExpressionStyleOption source, ExpressionStyleOption destination)
        {
            if (source == null || destination == null) 
                return;

            destination.FormatConditionType = source.FormatConditionType;
            destination.Background = source.Background;
            destination.Foreground = source.Foreground;
            destination.FontStyle = source.FontStyle;
            destination.FontFamily = source.FontFamily;
            destination.FontSize = source.FontSize;
            destination.FontWeight = source.FontWeight;
            destination.ExpressionString = source.ExpressionString;
            destination.FieldName = source.FieldName;
            destination.ApplyToRow = source.ApplyToRow;
            destination.Name = source.Name;
            destination.PredefinedFormatName = source.PredefinedFormatName;
            destination.Threshold = source.Threshold;
            destination.Comment = source.Comment;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
