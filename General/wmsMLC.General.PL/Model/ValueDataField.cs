using MLC.Ext.Common.Model.Domains;

namespace wmsMLC.General.PL.Model
{
    /// <summary>
    /// Класс, описывающий поля сущности.
    /// </summary>
    public class ValueDataField : DataField
    {
        public ValueDataField()
        {
            LookupButtonEnabled = true;
            IsEnabled = true;
            Visible = true;
        }

        public ValueDataField(DataField obj) : this()
        {
            AllowAddNewValue = obj.AllowAddNewValue;
            BindingPath = obj.BindingPath;
            Caption = obj.Caption;
            Description = obj.Description;
            DisplayFormat = obj.DisplayFormat;
            FieldName = obj.FieldName;
            FieldType = obj.FieldType;
            IsChangeLookupCode = obj.IsChangeLookupCode;
            IsMemoView = obj.IsMemoView;
            KeyLink = obj.KeyLink;
            LookupCode = obj.LookupCode;
            LookupFilterExt = obj.LookupFilterExt;
            LookupVarFilterExt = obj.LookupVarFilterExt;
            Name = obj.Name;
            SourceName = obj.SourceName;
        }

        public int Order { get; set; }

        public object Value { get; set; }

        /// <summary>
        /// Если true, то при вводе Enter выходим из диалога.
        /// </summary>
        public bool CloseDialog { get; set; }

        public bool SetFocus { get; set; }

        public string LabelPosition { get; set; }

        public bool IsLabelFontWeightBold { get; set; }

        public bool IsRequired { get; set; }

        public string EventName { get; set; }

        public string ImageName { get; set; }

        public string[] DependentFields { get; set; }

        /// <summary>
        /// Признак отслеживания изменений значения параметра
        /// </summary>
        public bool IsOnPropertyChange { get; set; }

        public override object Clone()
        {
            var result = Clone(new ValueDataField());
            return result;
        }

        public string HotKey { get; set; }
    }

    public class DataFieldWithDomain : ValueDataField
    {
        public Domain Domain { get; set; }
    }

    public static class ValueDataFieldConstants
    {
        public const string None = "None";
        public const string WfVariableFlag = "@";
        public const string WfPropertyName = "__systemWfProperty_";
        public const string RowNumberFlag = "__%ROWNUM%_";
        public const string FooterMenu = "FooterMenu";
        public const string LookupType = "LookupType";
        public const string MaxRows = "MaxRows";
        public const string MaxRowsOnPage = "MaxRowsOnPage";
        public const string CloseDialogOnSelectedItemChanged = "CloseDialogOnSelectedItemChanged";
        public const string UseFunctionKeys = "UseFunctionKeys";
        public const string ParentKeyPreview = "ParentKeyPreview";
        public const string HotKey = "HotKey";
        public const string HotKey2 = "HotKey2";
        public const string CustomDisplayMember = "CustomDisplayMember";
        public const string IsSubImageView = "IsSubImageView";
        public const string ItemsSource = "ItemsSource";
        public const string ValueMember = "ValueMember";
        public const string DisplayMember = "DisplayMember";
        public const string NavigationDirectionOnGotFocus = "NavigationDirectionOnGotFocus";
        public const string BindingIValueConverter = "BindingIValueConverter";
        public const string Parameter = "Parameter";
        public const string PropertiesBinding = "PropertiesBinding";
        public const string UseSpinEdit = "UseSpinEdit";
        public const string NotUseLookupAttribute = "NotUseLookupAttribute";
        public const string IsMergedProperty = "IsMergedProperty";
        public const string Row = "Row";
        public const string Column = "Column";
        public const string Value = "Value";
        public const string FontSize = "FontSize";
        public const string ShowControlMenu = "ShowControlMenu";
        public const string IsDoNotMovedFocusOnNextControl = "IsDoNotMovedFocusOnNextControl";
        public const string Fields = "Fields";
        public const string DisplaySetting = "DisplaySetting";
        public const string SuffixText = "SuffixText";
        public const string ShowAutoFilterRow = "ShowAutoFilterRow";
        public const string AllowAutoShowAutoFilterRow = "AllowAutoShowAutoFilterRow";
        public const string DoNotActionOnEnterKey = "DoNotActionOnEnterKey";
        public const string Properties = "Properties";
        public const string DoNotAllowUpdateShowAutoFilterRowFromXml = "DoNotAllowUpdateShowAutoFilterRowFromXml";
        public const string IsNotMenuButton = "IsNotMenuButton";
        public const string TransferHotKeyToControls = "TransferHotKeyToControls";
        public const string ShowMenu = "ShowMenu";
        public const string Command = "Command";
        public const string ShowTotalRow = "ShowTotalRow";

        #region GridColumn
        public const string ColumnWidth = "ColumnWidth";
        public const string ColumnBestFitMode = "ColumnBestFitMode";
        public const string BestFitColumnNames = "BestFitColumnNames";

        public const string FormatConditions = "FormatConditions";
        #endregion GridColumn
        #region LayoutGroup
        public const string LayoutGroupName = "LayoutGroupName";
        //public const string LayoutGroupOrientation = "LayoutGroupOrientation";
        //public const string LayoutGroupItemLabelsAlignment = "LayoutGroupItemLabelsAlignment";
        #endregion LayoutGroup

        public static bool ValidateUseWfVariable(object value)
        {
            if (!(value is string))
                return false;

            var text = value.ToString();
            if (string.IsNullOrEmpty(text))
                return false;

            return text.StartsWith(WfVariableFlag) &&
                   (text.Length == 1 || text.Length > 1 && text.Substring(1, 1) != WfVariableFlag);
        }

        public static string GetWfVariablePropertyName(object value)
        {
            if (!(value is string))
                return null;

            var text = value.ToString();
            if (string.IsNullOrEmpty(text))
                return text;

            return ValidateUseWfVariable(text) ? text.Substring(WfVariableFlag.Length) : text;
        }

        public static ValueDataField CreateDefaultParameter(string parameter)
        {
            return new ValueDataField
            {
                Name = parameter,
                Value = parameter
            };
        }
    }

    public class FormatConditionInfo
    {
        public string FieldName { get; set; }
        public string Expression { get; set; }
        public string PredefinedFormatName { get; set; }
        public bool ApplyToRow { get; set; }
        // TODO: добавить все, что нужно для переноса на DevExp
    }
}
