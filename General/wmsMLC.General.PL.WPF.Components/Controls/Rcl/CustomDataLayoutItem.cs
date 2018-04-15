using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using DevExpress.Mvvm.UI.Native.ViewGenerator;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.LayoutControl;
using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Grid;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Attributes;
using wmsMLC.General.PL.WPF.Components.Helpers;
using wmsMLC.General.PL.WPF.Components.Views;
using wmsMLC.General.PL.WPF.Helpers;
using Action = System.Action;
using Label = System.Windows.Controls.Label;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomDataLayoutItem : DataLayoutItem
    {
        #region .  Fields  .
        private const string WfDesignModeLookUpCodeEditorFilterExt = "ROWNUM <= 7";
        #endregion .  Fields  .

        public CustomDataLayoutItem()
        {
            LookupType = RclLookupType.None;
            LabelVisibility = Visibility.Visible;
            ShowTotalRow = true;
        }

        public CustomDataLayoutItem(bool isWfDesignMode, ValueDataField field)
            : this()
        {
            IsWfDesignMode = isWfDesignMode;
            Name = field.Name;

            LabelPosition = field.LabelPosition.To(LayoutItemLabelPosition.Left);
            if (field.LabelPosition.EqIgnoreCase(ValueDataFieldConstants.None))
                LabelVisibility = Visibility.Collapsed;

            Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed;
            DisplayFormat = field.DisplayFormat;
            IsReadOnly = !(field.IsEnabled.HasValue && field.IsEnabled.Value);

            DisplayName = field.Caption;
            FieldType = field.FieldType;
            LookUpCode = field.LookupCode;
            LookUpFilterExt = field.LookupFilterExt;
            CloseDialog = field.CloseDialog;
            SetFocus = field.SetFocus;
            BackGroundColor = field.BackGroundColor;

            IsMemoView = field.IsMemoView;

            Binding = new Binding(string.IsNullOrEmpty(field.FieldName) ? field.Name : field.FieldName)
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnDataErrors = true
            };

            //Проставляем Properties
            InitializeProperties(field);
        }

        #region .  Properties  .
        public static readonly DependencyProperty FieldTypeProperty =
            DependencyProperty.Register("FieldType", typeof(Type), typeof(CustomDataLayoutItem), new PropertyMetadata(default(Type)));
        public static readonly DependencyProperty LookUpCodeProperty =
            DependencyProperty.Register("LookUpCode", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty KeyLinkProperty =
            DependencyProperty.Register("KeyLink", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DisplayFormatProperty =
            DependencyProperty.Register("DisplayFormat", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackGroundColorPropertyName  =
            DependencyProperty.Register("BackGroundColor", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));

        /// <summary>
        /// Тип данных (по этому типу подбирается редактор)
        /// </summary>
        public Type FieldType
        {
            get { return (Type)GetValue(FieldTypeProperty); }
            set { SetValue(FieldTypeProperty, value); }
        }

        public string LookUpCode
        {
            get { return (string)GetValue(LookUpCodeProperty); }
            set { SetValue(LookUpCodeProperty, value); }
        }

        public string KeyLink
        {
            get { return (string)GetValue(KeyLinkProperty); }
            set { SetValue(KeyLinkProperty, value); }
        }

        public string DisplayFormat
        {
            get { return (string)GetValue(DisplayFormatProperty); }
            set { SetValue(DisplayFormatProperty, value); }
        }

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public string BackGroundColor
        {
            get { return (string)GetValue(BackGroundColorPropertyName); }
            set { SetValue(BackGroundColorPropertyName, value); }
        }

        public string LookUpFilterExt { get; set; }

        /// <summary>
        /// Не использовать DetailVisibleAttribute. Свойство Visibility устанавливается извне.
        /// </summary>
        public bool IsVisibilitySetOutside { get; set; }

        /// <summary>
        /// Не использовать DetailDisplayFormatAttribute. Свойство DisplayFormat устанавливается извне.
        /// </summary>
        public bool IsDisplayFormatSetOutside { get; set; }

        /// <summary>
        /// Свойство IsReadOnly устанавливается извне.
        /// </summary>
        public bool IsReadOnlySetOutside { get; set; }

         public bool IsMemoView { get; set; }

        //TODO: понять что это такое и как используется
        public object ParentViewModelSource { get; set; }

        public bool IsLabelFontWeightBold { get; set; }

        public Visibility LabelVisibility { get; set; }

        /// <summary>
        /// Если true, то при вводе Enter выходим из диалога.
        /// </summary>
        public bool CloseDialog { get; set; }

        /// <summary>
        /// Если true, то при вводе Enter не переходим на следующий контрол.
        /// </summary>
        public bool IsDoNotMovedFocusOnNextControl { get; set; }

        public bool SetFocus { get; set; }

        /// <summary>
        /// Не показываем tooltip.
        /// </summary>
        public bool TooltipDisable { get; set; }

        public bool IsLastElement { get; set; }
        public ICommand CloseDialogCommand { get; set; }
        public int? MaxRows { get; set; }
        public int? MaxRowsOnPage { get; set; }
        public bool CloseDialogOnSelectedItemChanged { get; set; }
        public bool UseFunctionKeys { get; set; }
        public bool ParentKeyPreview { get; set; }
        public RclLookupType LookupType { get; set; }
        public Key HotKey { get; set; }
        public string CustomDisplayMember { get; set; }
        public IList ItemsSource { get; set; }
        public string ValueMember { get; set; }
        public string DisplayMember { get; set; }
        public bool IsWfDesignMode { get; set; }
        public ValueDataField[] FooterMenu { get; set; }
        public bool? ShowControlMenu { get; set; }
        public DataField[] Fields { get; set; }
        public SettingDisplay? DisplaySetting { get; set; }
        public bool ShowAutoFilterRow { get; set; }
        public bool AllowAutoShowAutoFilterRow { get; set; }
        public bool ShowTotalRow { get; set; }
        public bool DoNotActionOnEnterKey { get; set; }
        public string[] BestFitColumnNames { get; set; }
        public IEnumerable<FormatConditionInfo> FormatConditions { get; set; }
        public bool DoNotAllowUpdateShowAutoFilterRowFromXml { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .

        private void InitializeProperties(ValueDataField field)
        {
            string svalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.LookupType, IsWfDesignMode, out svalue))
                LookupType = (RclLookupType)Enum.Parse(typeof(RclLookupType), svalue);

            int ivalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.MaxRows, IsWfDesignMode, out ivalue))
                MaxRows = ivalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.MaxRowsOnPage, IsWfDesignMode, out ivalue))
                MaxRowsOnPage = ivalue;

            bool bvalue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.CloseDialogOnSelectedItemChanged,
                IsWfDesignMode, out bvalue))
                CloseDialogOnSelectedItemChanged = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.UseFunctionKeys, IsWfDesignMode,
                out bvalue))
                UseFunctionKeys = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ParentKeyPreview, IsWfDesignMode,
                out bvalue))
                ParentKeyPreview = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.HotKey, IsWfDesignMode, out svalue))
                HotKey = (Key)Enum.Parse(typeof(Key), svalue);

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.CustomDisplayMember, IsWfDesignMode,
                out svalue))
                CustomDisplayMember = svalue;

            object listValue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ItemsSource, IsWfDesignMode,
                out listValue))
            {
                var list = listValue as IList;
                if (list == null)
                {
                    var listSource = listValue as IListSource;
                    if (listSource != null)
                        list = listSource.GetList();
                }
                
                ItemsSource = list;
            }

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ValueMember, IsWfDesignMode, out svalue))
                ValueMember = svalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.DisplayMember, IsWfDesignMode, out svalue))
                DisplayMember = svalue;

            ValueDataField[] valueDataFields;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.FooterMenu, IsWfDesignMode,
                out valueDataFields))
                FooterMenu = valueDataFields;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ShowControlMenu, IsWfDesignMode,
                out bvalue))
                ShowControlMenu = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.IsDoNotMovedFocusOnNextControl,
                IsWfDesignMode, out bvalue))
                IsDoNotMovedFocusOnNextControl = bvalue;

            DataField[] dataFields;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.Fields, IsWfDesignMode, out dataFields))
                Fields = dataFields;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.DisplaySetting, IsWfDesignMode,
                out svalue))
                DisplaySetting = (SettingDisplay)Enum.Parse(typeof(SettingDisplay), svalue);

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ShowAutoFilterRow, IsWfDesignMode,
                out bvalue))
                ShowAutoFilterRow = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.AllowAutoShowAutoFilterRow,
                IsWfDesignMode,
                out bvalue))
                AllowAutoShowAutoFilterRow = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ShowTotalRow,
                IsWfDesignMode,
                out bvalue))
                ShowTotalRow = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.DoNotActionOnEnterKey,
                IsWfDesignMode,
                out bvalue))
                DoNotActionOnEnterKey = bvalue;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.DoNotAllowUpdateShowAutoFilterRowFromXml,
                IsWfDesignMode,
                out bvalue))
                DoNotAllowUpdateShowAutoFilterRowFromXml = bvalue;

            string[] bestFitColumnNames;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.BestFitColumnNames, IsWfDesignMode,
                out bestFitColumnNames))
                BestFitColumnNames = bestFitColumnNames;

            IEnumerable<FormatConditionInfo> formatConditions;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.FormatConditions, true,
                out formatConditions))
                FormatConditions = formatConditions;
        }

        protected override bool GetIsLabelVisible(object label)
        {
            return base.GetIsLabelVisible(label) && LabelVisibility == Visibility.Visible;
        }

        protected override object GetLabel()
        {
            // Если label задали вручную - ничего не меняем
            if (!string.IsNullOrEmpty(Label as string))
            {
                return new TextBlock
                {
                    Text = Label as string
                };
            }

            return Label;
            //return base.GetLabel();
        }

        protected override object GetToolTip()
        {
            // чтобы не показывался пустой прямоугольник - сбрасываем ToolTip в null
            if (ToolTip is string && (string) ToolTip == string.Empty)
                ToolTip = null;

            return ToolTip;
            //return base.GetToolTip();
        }

        //Нет в 13.2
        //protected override void InitializeUI(Type valueType, DataType? dataType)
        //{
        //    // Если content задали вручную - ничего не меняем
        //    if (Content == null)
        //        base.InitializeUI(valueType, dataType);
        //}

        private PropertyDataType? GetPropertyDataType()
        {
            if (PropertyInfo == null || PropertyInfo.Attributes == null)
                return null;

            //return PropertyInfo.Attributes.PropertyDataType; 13.2
            return PropertyInfo.Attributes.PropertyDataType();
        }

        //Нет в 13.2
        //protected override FrameworkElement CreateContent(Type valueType, DataType? dataType)
        protected override FrameworkElement CreateContentAndInitializeUI()
        {
            FrameworkElement result = null;
            if (!string.IsNullOrEmpty(LookUpCode) && LookupType == RclLookupType.None)
                LookupType = RclLookupType.Default;

            switch (LookupType)
            {
                case RclLookupType.Default:
                case RclLookupType.DefaultGrid:
                    var customComboBoxEditRcl = new CustomComboBoxEditRcl
                    {
                        Name = string.Format("{0}_{1}", Name, LookupType),
                        IsWfDesignMode = IsWfDesignMode,
                        LookupType = LookupType,
                        LookUpCodeEditorFilterExt =
                            IsWfDesignMode ? WfDesignModeLookUpCodeEditorFilterExt : LookUpFilterExt,
                        LookUpCodeEditor = LookUpCode,
                        UseFunctionKeys = UseFunctionKeys,
                        ParentKeyPreview = ParentKeyPreview,
                        HotKey = HotKey,
                        CustomDisplayMember = CustomDisplayMember,
                        LookupTitle = DisplayName
                    };
                    if (MaxRowsOnPage.HasValue)
                        customComboBoxEditRcl.MaxRowsOnPage = MaxRowsOnPage.Value;
                    result = customComboBoxEditRcl;
                    break;
                case RclLookupType.SelectControl:
                    VerticalAlignment = VerticalAlignment.Stretch;
                    var select = new CustomSelectControl
                    {
                        LookUpCodeEditorFilterExt =
                            IsWfDesignMode ? WfDesignModeLookUpCodeEditorFilterExt : LookUpFilterExt,
                        LookUpCodeEditor = LookUpCode,
                        UseFunctionKeys = UseFunctionKeys,
                        ParentKeyPreview = ParentKeyPreview,
                        CustomDisplayMember = CustomDisplayMember,
                        ExternalItemsSource = ItemsSource,
                        DisplayMember = DisplayMember,
                        ValueMember = ValueMember
                    };
                    if (MaxRowsOnPage.HasValue)
                        select.MaxRowsOnPage = MaxRowsOnPage.Value;
                    if ((CloseDialogOnSelectedItemChanged || CloseDialog) && CloseDialogCommand != null)
                    {
                        select.CommandParameter = ValueDataFieldConstants.CreateDefaultParameter(ValueDataFieldConstants.Value);
                        select.Command = CloseDialogCommand;
                    }
                    result = select;
                    break;
                case RclLookupType.SelectGridControl:
                case RclLookupType.List:
                    VerticalAlignment = VerticalAlignment.Stretch;

                    //debug
                    //if (IsWfDesignMode)
                    //    Fields = GetGridFields();
                    //

                    var customSelectView = new CustomSelectView
                    {
                        ItemsSourceType = FieldType,
                        IsWfDesignMode = IsWfDesignMode,
                        AutoShowAutoFilterRowWhenRowsCountMoreThan = -1,
                        LookUpCodeEditorFilterExt =
                            IsWfDesignMode ? WfDesignModeLookUpCodeEditorFilterExt : LookUpFilterExt,
                        LookUpCodeEditor = LookUpCode,
                        Source = ItemsSource,
                        DisplaySetting = DisplaySetting.HasValue ? DisplaySetting.Value : SettingDisplay.List,
                        VerifyColumnsSourceChanged = true,
                        ShowSelectMenuItem = LookupType == RclLookupType.SelectGridControl,
                        MaxRows = MaxRows,
                        Fields = Fields != null && Fields.Any() ? new ObservableCollection<DataField>(Fields) : null,
                        AllowAutoShowAutoFilterRow = AllowAutoShowAutoFilterRow,
                        DoNotAllowUpdateShowAutoFilterRowFromXml = DoNotAllowUpdateShowAutoFilterRowFromXml,
                        ShowAutoFilterRow = ShowAutoFilterRow,
                        ShowTotalRow = ShowTotalRow,
                        DoNotActionOnEnterKey = DoNotActionOnEnterKey,
                        BestFitColumnNames = BestFitColumnNames,
                        FormatConditions = FormatConditions
                    };

                    if (ShowControlMenu.HasValue)
                        customSelectView.ShowMenu = ShowControlMenu.Value;

                    if (IsWfDesignMode && string.IsNullOrEmpty(customSelectView.LookUpCodeEditor) &&
                        customSelectView.Source == null && FieldType != null)
                    {
                        customSelectView.Source = new[] {Activator.CreateInstance(FieldType)};
                    }

                    if (FooterMenu != null && FooterMenu.Length > 0)
                        customSelectView.AddFooterMenuItems(FooterMenu, FontSize, IsWfDesignMode);

                    if ((CloseDialogOnSelectedItemChanged || CloseDialog) && CloseDialogCommand != null)
                    {
                        customSelectView.Command = CloseDialogCommand;
                    }

                    result = customSelectView;
                    break;
            }

            if (IsMemoView)
            {
                result = new MemoEdit
                {
                    ShowIcon = false, 
                    //PopupWidth = 250, 
                    MemoTextWrapping = TextWrapping.Wrap,
                    MemoVerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    MemoHorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
                };
            }

            if (typeof(IList).IsAssignableFrom(FieldType))
                result = CreateContentSubListView(FieldType);

            if (result == null)
                result = CreateContentInternal();

            if (result == null)
            {
                result = base.CreateContentAndInitializeUI();
                var textEdit = result as TextEdit;
                if (textEdit != null)
                {
                    textEdit.TextWrapping = TextWrapping.Wrap;
                    if (textEdit.TextWrapping == TextWrapping.Wrap)
                    {
                        textEdit.HorizontalContentAlignment = HorizontalAlignment.Left;
                        textEdit.VerticalContentAlignment = VerticalAlignment.Top;
                    }
                    if (!String.IsNullOrEmpty(BackGroundColor))
                    {
                        var brushconverter = new BrushConverter();
                        ((TextEdit)result).Background = brushconverter.ConvertFromString(BackGroundColor) as Brush;
                    }
                }
            }

            var termilalUi = result as IRclUi;
            if (termilalUi == null)
            {
                //Гридам не навязывать стандартную обработку клавиш.
                if (LookupType != RclLookupType.SelectGridControl && LookupType != RclLookupType.List)
                {
                    if (CloseDialog)
                        KeyHelper.SetCloseDialogElement(result);
                    if (IsDoNotMovedFocusOnNextControl)
                        KeyHelper.SetDoNotMovedFocusOnNextControl(result);
                    result.PreviewKeyDown += (s, e) => KeyHelper.PreviewKeyDown(result, e);
                }
            }
            else
            {
                termilalUi.IsMovedFocusOnNextControl = !CloseDialog;
            }

            if (IsFocused || SetFocus)
                result.BackgroundFocus();

            if (IsWfDesignMode)
            {
                var themeName = ThemeManager.GetThemeName(this);
                if (string.IsNullOrEmpty(themeName))
                    themeName = StyleKeys.RclDefaultThemeName;
                ThemeManager.SetThemeName(result, themeName);
            }

            return result;
        }

        private FrameworkElement CreateContentInternal()
        {
            var flag = FieldType.IsNullable();
            var fieldtype = FieldType.GetNonNullableType();

            if (fieldtype == typeof(bool))
                return new CheckEdit();

            if (fieldtype == typeof (DateTime))
                return new CustomDateEdit {AllowNullInput = flag, MaskType = MaskType.DateTimeAdvancingCaret};

            if (fieldtype.IsPrimitive || fieldtype == typeof(decimal))
            {
                var edit = new CustomTextEdit
                {
                    AllowNullInput = flag,
                    MaskType = MaskType.Numeric
                };
                if (flag)
                    edit.EditValueType = FieldType;

                var dataType = GetPropertyDataType();
                if (dataType.HasValue && (dataType.Value == PropertyDataType.Currency))
                {
                    edit.Mask = "C";
                    edit.MaskUseAsDisplayFormat = true;
                    edit.HorizontalContentAlignment = CurrencyValueAlignment;
                }
                return edit;
            }
            return null;
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            var property = GetBindedProperty(newValue);
            // Т.к. пересоздание объекта происходит при любых изменениях Layout'а, то первоначальную инициализацию делаем один раз
            if (string.IsNullOrEmpty(Name))
            {
                if (property != null)
                {
                    Name = property.Name;
                    // NOTE: без этой регистрации не работает сохранение настроек
                    if (FindName(Name) == null)
                        RegisterName(Name, this);

                    FieldType = property.PropertyType;

                    // определяем видимость
                    if (!IsVisibilitySetOutside)
                    {
                        var visibleAttibute = property.Attributes[typeof(DetailVisibleAttribute)] as DetailVisibleAttribute;
                        if (visibleAttibute != null && !visibleAttibute.Visible)
                            Visibility = Visibility.Hidden;
                    }

                    // определяем формат
                    if (!IsDisplayFormatSetOutside)
                    {
                        var formatAttribute = property.Attributes[typeof(DetailDisplayFormatAttribute)] as BaseFormatAttribute
                            ?? property.Attributes[typeof(DefaultDisplayFormatAttribute)] as BaseFormatAttribute;
                        if (formatAttribute != null)
                            DisplayFormat = formatAttribute.DisplayFormat;
                    }

                    // определяем, что перед нами Lookup
                    var lookupAttribute = property.Attributes[typeof(LookupAttribute)] as LookupAttribute;
                    if (lookupAttribute != null)
                    {
                        LookUpCode = lookupAttribute.LookUp;
                        KeyLink = lookupAttribute.KeyLink;
                    }

                    DisplayName = property.DisplayName;

                    if (!TooltipDisable)
                        ToolTip = property.Description;

                    if (!IsReadOnlySetOutside)
                        IsReadOnly = property.IsReadOnly;
                }
            }

            // запускаем базовый движок. После этого все контролы пересоздадутся.
            base.OnDataContextChanged(oldValue, newValue);

            Control label;

            if (Content is SubListView && !IsReadOnly)
            {
                label = new CustomLabelItem
                    {
                        Content = DisplayName,
                        ParentData = DataContext as IValidatable,
                        PropertyName = Name,
                        ToolTip = ToolTip,
                    };
                Label = label;
            }
            else
            {
                label = new Label
                {
                    Content = DisplayName
                };
            }

            if (IsLabelFontWeightBold)
                label.FontWeight = FontWeights.Bold;

            label.FontSize = FontSize;
            Label = label;

            // Снова все настраиваем

            //ToolTip = Description;
            //IsReadOnly = IsPropertyReadOnly;

            var isNew = DataContext as BL.IIsNew;
            if (isNew != null)
            {
                if (isNew.IsNew)
                {
                    if (!IsReadOnlySetOutside)
                        IsReadOnly = !EnableCreateAttribute.DefaultEnableCreate;

                    if (property != null)
                    {
                        var enableCreateAttibute = property.Attributes[typeof(EnableCreateAttribute)] as EnableCreateAttribute;
                        if (enableCreateAttibute != null)
                        {
                            if (!IsReadOnlySetOutside)
                                IsReadOnly = !enableCreateAttibute.Enable;
                        }
                    }
                }
                else
                {
                    if (!IsReadOnlySetOutside)
                        IsReadOnly = !EnableEditAttribute.DefaultEnableEdit;

                    if (property != null)
                    {
                        var enableEditAttribute = property.Attributes[typeof(EnableEditAttribute)] as EnableEditAttribute;
                        if (enableEditAttribute != null)
                        {
                            if (!IsReadOnlySetOutside)
                                IsReadOnly = !enableEditAttribute.Enable;
                        }
                    }
                }
            }

            if (Content is SubListView)
                ((SubListView) Content).IsReadOnly = IsReadOnly;

            if (Binding != null)
            {
                if (IsReadOnly)
                {
                    Binding.Mode = BindingMode.OneWay;
                }
                else
                {
                    Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                }
            }

            ApplyProperties();
        }

        protected override DependencyProperty GetContentValueProperty(FrameworkElement content)
        {
            var res = base.GetContentValueProperty(content);
            if (res == null)
            {
                if (content is SubListView)
                    return SubListView.SourceProperty;

                if (content is CustomSelectControl)
                    return CustomSelectControlBase.EditValueProperty;

                if (content is CustomSelectView)
                    return CustomSelectView.EditValueProperty;
            }

            return res;
        }

        private string[] GetBindingPath()
        {
            string path;
            if (Binding == null || Binding.Path == null || string.IsNullOrEmpty(path = Binding.Path.Path))
                return null;
            return path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private PropertyDescriptor GetBindedProperty(object obj)
        {
            if (obj == null) 
                return null;

            var pathes = GetBindingPath();
            if (pathes == null)
                return null;

            var type = obj.GetType();
            PropertyDescriptor property = null;
            foreach (var item in pathes)
            {
                var properties = TypeDescriptor.GetProperties(type);
                property = properties.Find(item, true);
                if (property == null)
                    return null;
                type = property.PropertyType;
            }

            return property;
        }

        private void ApplyProperties()
        {
            //HACK: получаем ограничение на контролы которыми можем управлять через данный биндинг - все они должны быть наследниками BaseEdit
            //      в противном случае форматы работать не будут.
            var be = Content as BaseEdit;
            if (be != null)
            {
                be.GotFocus += BaseEditOnGotFocus;
                be.IsReadOnly = IsReadOnly;
                if (!string.IsNullOrEmpty(DisplayFormat))
                {
                    be.DisplayFormatString = DisplayFormat;
                }
            }

            var te = Content as TextEdit;
            if (te != null && !string.IsNullOrEmpty(DisplayFormat))
            {
                te.Mask = DisplayFormat;
                te.MaskUseAsDisplayFormat = true;
            }
        }

        private void BaseEditOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var baseEdit = sender as BaseEdit;
            if (baseEdit == null)
                return;
            Dispatcher.BeginInvoke((Action)(baseEdit.SelectAll));
        }

        protected override bool GetIsActuallyReadOnly()
        {
            return IsReadOnly;
        }

        private static FrameworkElement CreateContentSubListView(Type type)
        {
            return new SubListView(type);
        }

        //////// debug
        //private DataField[] GetGridFields()
        //{
        //    var gridFieldNames = new[] { "SKUID_R_NAME", "PRODUCTCOUNTSKU", "PRODUCTCOUNT", "VARTDESC", "VQLFNAME" };
        //    var gridFields = GetGridFields(typeof(Product), SettingDisplay.List, gridFieldNames);

        //    var selectField = new DataField
        //    {
        //        Name = Product.IsSelectedPropertyName,
        //        BindingPath = Product.IsSelectedPropertyName,
        //        Caption = string.Empty,
        //        EnableEdit = false,
        //        IsEnabled = true,
        //        FieldName = Product.IsSelectedPropertyName,
        //        FieldType = typeof(bool),
        //        SourceName = Product.IsSelectedPropertyName
        //    };
        //    gridFields.Insert(0, selectField);
        //    return gridFields.ToArray();
        //}
        
        //private List<DataField> GetGridFields(Type type, SettingDisplay displaySetting, string[] propertyNames)
        //{
        //    var fieldList = DataFieldHelper.Instance.GetDataFields(type, displaySetting);
        //    if (propertyNames == null)
        //        return new List<DataField>(fieldList);

        //    return propertyNames.Select(propertyName => fieldList.FirstOrDefault(p => p.Name.EqIgnoreCase(propertyName)))
        //            .Where(field => field != null)
        //            .ToList();
        //}
        //////// debug

        #endregion
    }

    public enum RclLookupType
    {
        None,
        Default,
        DefaultGrid,
        SelectControl,
        SelectGridControl,
        List
    }
}