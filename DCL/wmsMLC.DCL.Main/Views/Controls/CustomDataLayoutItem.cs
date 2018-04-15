// TODO: убрать HACK, когда DevExp ответят на https://www.devexpress.com/Support/Center/Question/Details/Q554377
// HACK: 2013.12.06 - проблемы из-за перехода DevExp c 13.1.6 до 13.2.5

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Native.ViewGenerator;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Validation;
using DevExpress.Xpf.LayoutControl;
using MLC.Ext.Common.Model.Domains;
using MLC.Ext.Wpf.Views.Controls.Editors;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.Views;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Attributes;
using wmsMLC.General.PL.WPF.Converters;
using wmsMLC.General.PL.WPF.Helpers;
using WebClient.Common.Types;
using Label = System.Windows.Controls.Label;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDataLayoutItem : DataLayoutItem, IDisposable
    {
        #region .  Fields  .
        // HACK: грязно, но работает
        bool _firstInit;
        private ButtonEdit _edit;

        Brush _formulaBackground = Brushes.Yellow;
        Binding _trueBinding;
        bool _trueAllowNullInput;
        Type _trueEditValueType;
        MaskType _truMaskType = MaskType.None;
        string _trueMask;
        bool _trueMaskUseAsDisplayFormat;
        HorizontalAlignment _trueHorizontalContentAlignment = HorizontalAlignment.Left;
        Brush _trueBackground;
        private DataField _field;
        #endregion .  Fields  .

        #region .  ctors  .

        public CustomDataLayoutItem()
        {
            LookupButtonEnabled = true;
            LabelVisibility = Visibility.Visible;
        }

        public CustomDataLayoutItem(DataField field) : this()
        {
            _field = field;
            Name = field.Name;
            FieldType = field.FieldType;
            DisplayName = field.Caption;
            ForceEnabled = field.IsEnabled;
            EnableCreate = field.EnableCreate;
            EnableEdit = field.EnableEdit;
            Visibility = field.Visible ? Visibility.Visible : Visibility.Collapsed;
            DisplayFormat = field.DisplayFormat;
            LookUpFilterExt = field.LookupFilterExt;
            LookUpVarFilterExt = field.LookupVarFilterExt;
            LookUpCode = field.LookupCode;
            KeyLink = field.KeyLink;
            IsMemoView = field.IsMemoView;
            BackGroundColor = field.BackGroundColor;
            DisplayTextConverter = field.DisplayTextConverter;

            var valueDataField = field as ValueDataField;
            if (valueDataField != null)
            {
                SetFocus = valueDataField.SetFocus;
                Key currKey;
                HotKey = Enum.TryParse(valueDataField.HotKey, out currKey) ? currKey : Key.None;
            }

            if (field.MaxLength.HasValue)
                MaxLength = field.MaxLength.Value;
            
            Binding = new Binding(string.IsNullOrEmpty(field.FieldName) ? field.Name : field.FieldName)
            {
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                ValidatesOnDataErrors = true
            };

            SetLabelProperties(valueDataField);

            IValueConverter valueConverter;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.BindingIValueConverter, false, out valueConverter))
                Binding.Converter = valueConverter;

            object objectValue;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.Parameter, false, out objectValue))
                Binding.ConverterParameter = objectValue;

            bool vbool;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.UseSpinEdit, false, out vbool))
                UseSpinEdit = vbool;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.IsSubImageView, false, out vbool))
                IsSubImageView = vbool;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.ShowMenu, false, out vbool))
                ShowMenu = vbool;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.NotUseLookupAttribute, false, out vbool))
                NotUseLookupAttribute = vbool;

            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.IsMergedProperty, false, out vbool))
                IsMergedProperty = vbool;

            double vdouble;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.FontSize, false, out vdouble))
                FontSize = vdouble;

            Dictionary<DependencyProperty, Binding> propertiesBinding;
            if (DataField.TryGetFieldProperties(field, ValueDataFieldConstants.PropertiesBinding, false, out propertiesBinding))
                PropertiesBinding = propertiesBinding;
        }
        #endregion .  ctors  .

        #region .  Properties  .
        public static readonly DependencyProperty FieldTypeProperty =
            DependencyProperty.Register("FieldType", typeof(Type), typeof(CustomDataLayoutItem), new PropertyMetadata(default(Type)));
        public static readonly DependencyProperty LookUpCodeProperty =
            DependencyProperty.Register("LookUpCode", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty LookUpFilterExtProperty =
            DependencyProperty.Register("LookUpFilterExt", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty LookUpVarFilterExtProperty =
            DependencyProperty.Register("LookUpVarFilterExt", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty KeyLinkProperty =
            DependencyProperty.Register("KeyLink", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DisplayFormatProperty =
            DependencyProperty.Register("DisplayFormat", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty MaxLengthNameProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(CustomDataLayoutItem), new PropertyMetadata(default(int)));
        public static readonly DependencyProperty IsMemoViewProperty =
            DependencyProperty.Register("IsMemoView", typeof(bool), typeof(CustomDataLayoutItem), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty LookupButtonEnabledProperty =
            DependencyProperty.Register("LookupButtonEnabled", typeof(bool), typeof(CustomDataLayoutItem), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty BackGroundColorProperty =
            DependencyProperty.Register("BackGroundColor", typeof(string), typeof(CustomDataLayoutItem), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SetFocusProperty =
            DependencyProperty.Register("SetFocus", typeof(bool), typeof(CustomDataLayoutItem), new PropertyMetadata(default(bool)));
        public static readonly DependencyProperty HotKeyProperty =
            DependencyProperty.Register("HotKey", typeof(Key), typeof(CustomDataLayoutItem), new PropertyMetadata(default(Key)));

        /// <summary>
        /// Тип данных (по этому типу подбирается редактор)
        /// </summary>
        public Type FieldType
        {
            get { return (Type)GetValue(FieldTypeProperty); }
            set { SetValue(FieldTypeProperty, value); }
        }

        public string FieldName
        {
            get { return (string)GetValue(FieldNameProperty); }
            set { SetValue(FieldNameProperty, value); }
        }

        public Key HotKey
        {
            get { return (Key)GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }

        public string BackGroundColor
        {
            get { return (string)GetValue(BackGroundColorProperty); }
            set { SetValue(BackGroundColorProperty, value); }
        }

        public string LookUpCode
        {
            get { return (string)GetValue(LookUpCodeProperty); }
            set { SetValue(LookUpCodeProperty, value); }
        }

        public string LookUpFilterExt
        {
            get { return (string)GetValue(LookUpFilterExtProperty); }
            set { SetValue(LookUpFilterExtProperty, value); }
        }

        public string LookUpVarFilterExt
        {
            get { return (string)GetValue(LookUpVarFilterExtProperty); }
            set { SetValue(LookUpVarFilterExtProperty, value); }
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

        public bool IsReadOnlyRightDependcy { get; set; }

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthNameProperty); }
            set { SetValue(MaxLengthNameProperty, value); }
        }

        public bool IsMemoView
        {
            get { return (bool)GetValue(IsMemoViewProperty); }
            set { SetValue(IsMemoViewProperty, value); }
        }

        public bool LookupButtonEnabled
        {
            get { return (bool)GetValue(LookupButtonEnabledProperty); }
            set { SetValue(LookupButtonEnabledProperty, value); }
        }

        public object ToolTipIns { get; set; }

        public IValueConverter DisplayTextConverter { get; set; }

        /// <summary>
        /// Не использовать DetailVisibleAttribute. Свойство Visibility устанавливается извне.
        /// </summary>
        public bool IsVisibilitySetOutside { get; set; }

        /// <summary>
        /// Не использовать DetailDisplayFormatAttribute. Свойство DisplayFormat устанавливается извне.
        /// </summary>
        public bool IsDisplayFormatSetOutside { get; set; }

        /// <summary>
        /// Привязка к значению, которая будет использовать в режиме задания формулы
        /// </summary>
        public Binding FormulaBinding { get; set; }

        public bool InFormulaMode
        {
            get { return (bool)GetValue(InFormulaModeProperty); }
            private set { SetValue(InFormulaModeProperty, value); }
        }

        public static readonly DependencyProperty InFormulaModeProperty = DependencyProperty.Register("InFormulaMode", typeof(bool), typeof(CustomDataLayoutItem));

        //TODO: понять что это такое и как используется
        public object ParentViewModelSource { get; set; }

        public bool IsLabelFontWeightBold { get; set; }

        public IModelHandler ParentViewModel { get; set; }

        /// <summary>
        /// Доступность поля при создании объекта
        /// </summary>
        public bool? EnableCreate { get; set; }

        /// <summary>
        /// Доступность поля при изменении объекта
        /// </summary>
        public bool? EnableEdit { get; set; }

        /// <summary>
        /// Доступность поля (игнорирует EnableCreate и EnableEdit)
        /// </summary>
        public bool? ForceEnabled { get; set; }

        private bool _isInitialized;

        public bool? IsMergedProperty { get; set; }

        /// <summary>
        /// Использовать SpinEdit редактор для числовых типов.
        /// </summary>
        public bool UseSpinEdit { get; set; }

        public Dictionary<DependencyProperty, Binding> PropertiesBinding { get; set; }

        /// <summary>
        /// Если значение свойства true не проверяем наличие LookupAttribute.
        /// </summary>
        public bool NotUseLookupAttribute { get; set; }

        public Visibility LabelVisibility { get; set; }

        public bool SetFocus 
        {
            get { return (bool)GetValue(SetFocusProperty); }
            set
            {
                SetValue(SetFocusProperty, value);
                if (value)
                Content.BackgroundFocus();
            }
        }

        private bool IsXmlLoading
        {
            get
            {
                if (Parent == null)
                    return false;
                var parent = VisualTreeHelperExt.GetLogicalParent<CustomDataLayoutControl>(this);
                return parent != null && parent.IsXmlLoading;
            }
        }

        public bool IsSubImageView { get; set; }
        public bool ShowMenu { get; set; }
        #endregion .  Properties  .

        #region .  Methods  .

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Parent == null || IsXmlLoading)
                return new Size();

            return base.MeasureOverride(availableSize);
        }

        protected override Size OnMeasure(Size availableSize)
        {
            if (Parent == null || IsXmlLoading)
                return new Size();

            return base.OnMeasure(availableSize);
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
                //return Label;
                return new TextBlock { Text = Label as string };
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

        private PropertyDataType? GetPropertyDataType()
        {
            if (PropertyInfo == null || PropertyInfo.Attributes == null)
                return null;

            return PropertyInfo.Attributes.PropertyDataType();
        }

        protected override FrameworkElement CreateContentAndInitializeUI()
        {
            var result = CreateContentAndInitializeUIInternal(() =>
            {
                var flag = FieldType.IsNullable();
                var fieldtype = FieldType.GetNonNullableType();

                if (fieldtype == typeof(bool))
                    return new CheckEdit();

                if (fieldtype == typeof(DateTime))
                    return new DateEdit { AllowNullInput = flag, MaskType = MaskType.DateTimeAdvancingCaret };

                if (fieldtype.IsPrimitive || fieldtype == typeof(decimal))
                {
                    var edit = new TextEdit
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

                return base.CreateContentAndInitializeUI();
            });
            if (result != null && PropertiesBinding != null)
            {
                foreach (var binding in PropertiesBinding)
                {
                    result.SetBinding(binding.Key, binding.Value);
                }
            }
            return result;
        }

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            if (Parent == null || IsXmlLoading)
                return;

            var property = GetBindedProperty(newValue);
            // Т.к. пересоздание объекта происходит при любых изменениях Layout-а, то первоначальную инициализацию делаем один раз
            if (!_isInitialized)
            {
                // NOTE: без этой регистрации не работает сохранение настроек
                if (FindName(Name) == null)
                    RegisterName(Name, this);

                if (property != null)
                {
                    if (FieldType == null)
                        FieldType = property.PropertyType;

                    if (string.IsNullOrEmpty(FieldName))
                        FieldName = property.Name;

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
                        else
                        {
                            if (property.PropertyType.GetNonNullableType() == typeof(DateTime))
                                DisplayFormat = "dd.MM.yyyy HH:mm:ss";
                        }
                    }

                    // определяем, что перед нами Lookup
                    if (!NotUseLookupAttribute)
                    {
                        var lookupAttribute = property.Attributes[typeof(LookupAttribute)] as LookupAttribute;
                        if (lookupAttribute != null)
                        {
                            if (LookUpCode == null)
                                LookUpCode = lookupAttribute.LookUp;

                            if (KeyLink == null)
                                KeyLink = lookupAttribute.KeyLink;
                        }
                    }

                    //var maxLengthAttribute = property.Attributes[typeof(MaxLengthAttribute)] as MaxLengthAttribute;
                    //if (maxLengthAttribute != null)
                    //    // +1 добавлен, чтобы сработала валидация при наступлении maxLength
                    //    MaxLength = maxLengthAttribute.MaxLength + 1;

                    //отключаем накручивание в полях TextEdit
                    //if (Content is TextEdit)
                    //    (Content as TextEdit).AllowSpinOnMouseWheel = false;
                }

                _isInitialized = true;

                // запускаем базовый движок. После этого все контролы пересоздадутся.
                base.OnDataContextChanged(oldValue, newValue);

                // оперделяем тежим работы SubListView
                var slv = (Content as ISubView);
                if (property != null && slv != null)
                    slv.ShouldUpdateSeparately = property.Attributes[typeof(XmlNotIgnoreAttribute)] == null;
            }

            Control label;

            if (Content is ISubView)
            {
                LabelPosition = LayoutItemLabelPosition.Top;
                label = new CustomLabelItem
                {
                    Content = DisplayName,
                    ParentData = DataContext as IValidatable,
                    PropertyName = Name,
                    ToolTip = ToolTipIns,
                    IsTabStop = false,
                    ShowValidationError = true,
                    FontWeight = FontWeights.ExtraBold,
                    Margin = new Thickness(0, 8, 0, 0)
                };

                if (ParentViewModel != null)
                    ((ISubView) Content).ParentViewModel = ParentViewModel;
            }
            else
            {
                label = new Label
                {
                    Content = DisplayName,
                    ToolTip = ToolTipIns
                };
            }

            label.FontSize = FontSize;

            if (IsLabelFontWeightBold)
                label.FontWeight = FontWeights.Bold;
            Label = label;

            // Снова все настраиваем
            var isReadOnly = IsReadOnly;
            var isNew = DataContext as IIsNew;
            if (isNew != null)
            {
                if (isNew.IsNew)
                    isReadOnly = !(EnableCreate.HasValue ? EnableCreate.Value : EnableCreateAttribute.DefaultEnableCreate);
                else
                    isReadOnly = !(EnableEdit.HasValue ? EnableEdit.Value : EnableEditAttribute.DefaultEnableEdit);
            }

            // если принудительно выставили разрешение на поле
            if (ForceEnabled.HasValue)
                isReadOnly = !ForceEnabled.Value;

            //Сводим изменение IsReadOnly к минимуму
            IsReadOnly = isReadOnly;

            if (Content is ButtonEdit && !(Content is PopupBaseEdit))
                (Content as ButtonEdit).ShowEditorButtons = !IsReadOnly;

            var read = IsReadOnly ? IsReadOnly : IsReadOnlyRightDependcy;

            if (Content is ISubView) 
                ((ISubView)Content).IsReadOnly = read;

            if (Binding != null)
            {
                if (IsReadOnly)
                    Binding.Mode = BindingMode.OneWay;
                else
                    Binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }

            ApplyProperties();
        }

        protected override void OnLabelChanged(object oldValue)
        {
            base.OnLabelChanged(oldValue);
            //Наименование элемента в тулбаре "Доступные элементы:" настройки элементов формы
            LayoutControl.SetCustomizationLabel(this, Label is Label ? ((Label)Label).Content : Label);
        }

        protected override DependencyProperty GetContentValueProperty(FrameworkElement content)
        {
            var res = base.GetContentValueProperty(content);

            if (res == null)
            {
                if (content is ISubView)
                    return ((ISubView) content).SourceProperty;
            }
            return res;
        }

        protected override void WriteCustomizablePropertiesToXML(XmlWriter xml)
        {
            WritePropertyToXML(this, xml, LabelProperty, "Label");
        }

        private FrameworkElement CreateContentAndInitializeUIInternal(Func<FrameworkElement> baseCreator)
        {
            var filterExp = string.Empty;
            const string propName = "MIUSEFILTER";
            // если контрол уже создан - незачем его пересоздавать. всю инициализацию нужно вынести перед
            if (Content != null)
                return Content as FrameworkElement;

            // если модель умеет создавать контролы
            var fieldProvider = ParentViewModel as IFieldProvider;
            if (fieldProvider != null)
            {
                // если у модели есть свой контрол для поля, то используем его
                var element = fieldProvider.GetElement(_field);
                if (element != null)
                    return element;
            }

            if (FieldName == propName)
            {
                var vm = ParentViewModel as ICustomFilterControlItemsProvider;
                var obj = DataContext as WMSBusinessObject;
                if (obj != null)
                {
                    if (obj.GetProperty(propName)!=null)
                    filterExp = obj.GetProperty(propName).ToString();
                }

                if (vm != null)
                    return new CustomFilterMemoEditControl(vm.GetFields(), filterExp);
            }

            if (!string.IsNullOrEmpty(LookUpCode) && !typeof(IList).IsAssignableFrom(FieldType))
            {
                var lookupInfo = LookupHelper.GetLookupInfo(LookUpCode);
                switch (lookupInfo.LookUpType)
                {
                    case LookupType.Default:
                        return new CustomLookUpEdit { Name = "clue" + Name, LookUpCodeEditorVarFilterExt = LookUpVarFilterExt, LookUpCodeEditorFilterExt = LookUpFilterExt, ParentViewModelSource = ParentViewModelSource, LookUpCodeEditor = LookUpCode, LookupButtonEnabled = LookupButtonEnabled };
                    case LookupType.Combobox:
                        return new CustomComboBoxEdit { LookUpCodeEditorVarFilterExt = LookUpVarFilterExt, LookUpCodeEditorFilterExt = LookUpFilterExt, ParentViewModelSource = ParentViewModelSource, LookUpCodeEditor = LookUpCode };
                    //case LookupType.LookupOpt:
                    //    return new CustomLookUpOpt { LookUpCodeEditorVarFilterExt = LookUpVarFilterExt, LookUpCodeEditorFilterExt = LookUpFilterExt, ParentViewModelSource = ParentViewModelSource, AllowAddNew = lookup.ObjectLookupAllowNew, LookUpCodeEditor = LookUpCode };
                    default:
                        throw new DeveloperException("Undefined value '{0}' in ObjectLookupSimple.", lookupInfo.LookUpType);
                }
            }

            if (IsSubImageView)
            {
                return new SubImageView {ParentViewModel = ParentViewModel, ShowMenu = ShowMenu};
            }

            if (typeof(IList).IsAssignableFrom(FieldType))
            {
                Type itemType = null;
                if (FieldType.IsGenericType)
                {
                    itemType = FieldType.GetGenericArguments().FirstOrDefault();
                    if (typeof(CustomParamValue).IsAssignableFrom(itemType))
                        return new CustomParamValueSubTreeView(FieldType, itemType) { ParentViewModel = ParentViewModel };

                    if (typeof(Entity2GC).IsAssignableFrom(itemType) && !string.IsNullOrEmpty(LookUpCode))
                        return new GCListView(FieldName, LookUpCode) { ParentViewModel = ParentViewModel };
                }
                return new SubListView(FieldType, itemType, FieldName) { ParentViewModel = ParentViewModel };
            }

            if (IsMemoView)
                return new MemoEdit
                {
                    ShowIcon = false,
                    PopupWidth = 250,
                    MemoTextWrapping = TextWrapping.Wrap,
                    MemoVerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

            if (!string.IsNullOrEmpty(DisplayFormat) && Regex.IsMatch(DisplayFormat, @"[\*\#\/]+$") && typeof(string).IsAssignableFrom(FieldType))
            {
                return new PasswordBoxEdit
                {
                    AllowNullInput = FieldType.IsNullable(),
                    EditValueType = FieldType.GetNonNullableType(),
                    ShowError = false,
                    ShowErrorToolTip = false,
                    InvalidValueBehavior = InvalidValueBehavior.AllowLeaveEditor,
                    NullText = KeyLink
                };
            }

            //вводим режим работы с формулами
            if (IsFieldFormulaEnable())
            {
                // создаем редактор с кнопкой
                _edit = new ButtonEdit
                {
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    AllowSpinOnMouseWheel = false
                };
                var notNullableType = FieldType.GetNonNullableType();
                if (notNullableType.IsPrimitive || notNullableType == typeof(decimal))
                {
                    _edit.AllowNullInput = FieldType.IsNullable();
                    _edit.EditValueType = notNullableType;
                    _edit.MaskType = MaskType.Numeric;

                    var dataType = GetPropertyDataType();

                    //if (dataType == DataType.Currency) так было в v.13.1
                    if (dataType == PropertyDataType.Currency)
                    {
                        _edit.Mask = "C";
                        _edit.MaskUseAsDisplayFormat = true;
                        _edit.HorizontalContentAlignment = CurrencyValueAlignment;
                    }
                }

                _edit.AllowDefaultButton = false;
                // создаем контекстное меню
                _edit.ContextMenuOpening += (s1, a1) =>
                {
                    if (_edit.ContextMenu == null)
                        return;

                    _edit.ContextMenu.Items.Clear();

                    if (!InFormulaMode)
                        return;

                    // добавляем св-ва
                    var properties = TypeDescriptor.GetProperties(DataContext.GetType()).Cast<PropertyDescriptor>();
                    foreach (var p in properties)
                    {
                        var mi = new MenuItem { Header = p.DisplayName };
                        mi.Click += (s2, a2) =>
                        {
                            Clipboard.SetText("${" + p.DisplayName + "}");
                            _edit.Paste();
                        };
                        _edit.ContextMenu.Items.Add(mi);
                    }
                    //TODO: добавляем переменные, которые уже определены
                };

                // создаем кнопку включения режима формулы
                var button = new ButtonInfo();
                button.GlyphKind = GlyphKind.Apply;
                //button.Content = new Image { Source = DCL.Resources.ImageResources.DCLDefault16.GetBitmapImage() };
                var label = new Label();
                label.Content = "Включение режима ввода формул.\n#{<name>;<range>} - ввод диапазона;\n${<name>} - использование переменной;\n\nФорматирование:\nFORMAT(\"%format expression%\", argument)";
                button.ToolTip = label;

                button.Click += FormulaModeButtonClick;

                _edit.Buttons.Add(button);
                return _edit;
            }

            // HACK: 2013.12.06 - проблемы из-за перехода DevExp c 13.1.6 до 13.2.5
            //var result = base.CreateContentAndInitializeUI();
            var result = CreateContentInternal(baseCreator);

            if (IsFocused || SetFocus)
                result.BackgroundFocus();

            if (result is DateEdit)
            {
                result = new CustomDateTimeEdit {AllowNullInput = true, AllowSpinOnMouseWheel = true };
            }
            else if (result is TextEdit)
            {
                // NOTE: всегда выравниваем текст по левому краю
                ((TextEdit)result).HorizontalContentAlignment = HorizontalAlignment.Left;
                // NOTE: отключаем накручивание в полях TextEdit
                ((TextEdit)result).AllowSpinOnMouseWheel = false;
                // NOTE: всегда умещать весь текст в контроле
                ((TextEdit)result).TextWrapping = TextWrapping.Wrap;
                if (String.IsNullOrEmpty(BackGroundColor)) return result;
                var brushconverter = new BrushConverter();
                ((TextEdit)result).Background = brushconverter.ConvertFromString(BackGroundColor) as Brush;
            }

            return result;
        }

        // HACK: 2013.12.06 - проблемы из-за перехода DevExp c 13.1.6 до 13.2.5
        private FrameworkElement CreateContentInternal(Func<FrameworkElement> baseCreator)
        {
            var valueType = FieldType.GetNonNullableType();

            // TODO: перейти на проверку по Domain-у
            if (FieldType == typeof (EntityReference))
            {
                var entityRefField = _field as DataFieldWithDomain;
                if (entityRefField == null)
                    throw new Exception(
                        string.Format("Тип поля EntityReference обязательно должен определяться через {0}. Тогда как у вас {1}.", typeof(DataFieldWithDomain).Name, _field));

                var entityRefDomain = entityRefField.Domain as EntityRefDomain;
                if (entityRefDomain == null)
                    throw new Exception(
                        "Тип поля EntityReference обязательно должен содержать EntityRefDomain, описывающий поведение");

                return new EntityRefEdit
                {
                    EntityRefDescriptor = entityRefDomain.EntityRefDescriptor,
                    IsEnabled = IsEnabled,
                    IsReadOnly = IsReadOnly
                };
            }

            if (UseSpinEdit && (valueType.IsPrimitive || valueType == typeof(decimal)))
            {
                var result = new CustomSpinEdit
                {
                    NotAllowSpin = true,
                    AllowNullInput = true,
                    AllowSpinOnMouseWheel = false,
                    ShowEditorButtons = false,
                    AllowDefaultButton = false,
                    IsFloatValue = false
                };

                result.IsFloatValue = (valueType == typeof (float) || valueType == typeof (double) ||
                                       valueType == typeof (decimal));
                return result;
            }

            //Базовый метод base.CreateContentAndInitializeUI() работает с PropertyInfo
            var underlyingClrType = PropertyInfo == null ? null : PropertyInfo.GetUnderlyingClrType();
             if (valueType == null || BindingSourceType == null || valueType == underlyingClrType)
                return base.CreateContentAndInitializeUI();

            return baseCreator();

            //HACK: DevExpress 14.1
            //var pd = GetBindedProperty(BindingSourceType);
            //if (pd == null)
            //{
            //    var pathes = GetBindingPath();
            //    var propname = (pathes == null || pathes.Length == 0) ? "NoMeanProperty" : pathes.Last();
            //    pd = TypeDescriptor.CreateProperty(BindingSourceType, propname, valueType);
            //}

            //            var generator = new CustomLayoutItemGenerator(this);
            //            var propertyInfo = (IEdmPropertyInfo)new EdmPropertyInfo(pd, new DataColumnAttributes(new AttributeCollection()), false);
            //            EditorsSource.GenerateEditor(propertyInfo, generator);
            //            return generator.Edit;
        }

        public void FormulaModeButtonClick(object sender, RoutedEventArgs e)
        {
            if (_edit == null)
                return;

            // запоминаем реальные параметры
            if (!_firstInit)
            {
                _firstInit = true;
                _trueBinding = BindingOperations.GetBinding(_edit, BaseEdit.EditValueProperty);
                _trueBackground = _edit.Background;
                _trueAllowNullInput = _edit.AllowNullInput;
                _trueEditValueType = _edit.EditValueType;
                _truMaskType = _edit.MaskType;
                _trueMask = _edit.Mask;
                _trueMaskUseAsDisplayFormat = _edit.MaskUseAsDisplayFormat;
                _trueHorizontalContentAlignment = _edit.HorizontalContentAlignment;
            }

            InFormulaMode = !InFormulaMode;
            var previosEditValue = _edit.EditValue;
            if (InFormulaMode)
            {
                _edit.ContextMenu = new ContextMenu();
                _edit.Background = _formulaBackground;
                //настраиваем редактор на тип string
                _edit.AllowNullInput = true;
                _edit.EditValueType = typeof(string);
                _edit.MaskType = MaskType.None;
                _edit.Mask = string.Empty;
                _edit.MaskUseAsDisplayFormat = false;
                _edit.HorizontalContentAlignment = HorizontalAlignment.Left;
                _edit.MaxLength = 0;

                // чистим Binding
                BindingOperations.ClearBinding(_edit, BaseEdit.EditValueProperty);
                // привязываем новый
                BindingOperations.SetBinding(_edit, BaseEdit.EditValueProperty, FormulaBinding);
            }
            else
            {
                _edit.ContextMenu = null;
                _edit.Background = _trueBackground;
                _edit.AllowNullInput = _trueAllowNullInput;
                _edit.EditValueType = _trueEditValueType;
                _edit.MaskType = _truMaskType;
                _edit.Mask = _trueMask;
                _edit.MaskUseAsDisplayFormat = _trueMaskUseAsDisplayFormat;
                _edit.HorizontalContentAlignment = _trueHorizontalContentAlignment;
                _edit.MaxLength = MaxLength;

                // чистим текущее значение формулы
                _edit.EditValue = null;
                // чистим Binding
                BindingOperations.ClearBinding(_edit, BaseEdit.EditValueProperty);
                // привязываем новый
                BindingOperations.SetBinding(_edit, BaseEdit.EditValueProperty, _trueBinding);
            }

            // выставляем значение
            _edit.EditValue = previosEditValue;
        }

        private bool IsFieldFormulaEnable()
        {
            if (FormulaBinding == null)
                return false;

            if (!string.IsNullOrEmpty(KeyLink))
                return false;
            
            var notNullableType = FieldType.GetNonNullableType();
            if (notNullableType == typeof(bool) || notNullableType == typeof(DateTime))
                return false;

            return true;
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

        public void ApplyProperties()
        {
            //HACK: получаем ограничение на контролы которыми можем управлять через данный биндинг - все они должны быть наследниками BaseEdit
            //      в противном случае форматы работать не будут.
            var be = Content as BaseEdit;
            var te = Content as TextEdit;
            if (be != null)
            {
                be.GotFocus -= BaseEditOnGotFocus;
                be.GotFocus += BaseEditOnGotFocus;
                be.IsReadOnly = IsReadOnly;
                if (!string.IsNullOrEmpty(DisplayFormat))
                {
                    be.DisplayFormatString = DisplayFormat;
                    if (te != null)
                    {
                        te.Mask = DisplayFormat;
                        te.MaskUseAsDisplayFormat = true;
                        // выставляем тип маски
                        // TODO: проверить как это отработает на custom форматах
                        var notNullableType = FieldType.GetNonNullableType();
                        if (notNullableType.IsNumeric())
                        {
                            te.MaskType = MaskType.Numeric;
                        }
                        else if (notNullableType == typeof (DateTime))
                        {
                            //te.MaskType = MaskType.DateTime;
                            te.MaskType = MaskType.DateTimeAdvancingCaret;
                        }
                        else
                        {
                            te.MaskType = MaskType.Simple;
                        }
                        if (MaxLength > 0)
                            te.MaxLength = MaxLength;
                     }
                }
                if (DisplayTextConverter != null && te != null)
                {
                    var conv = Binding.Converter as IConverterSI;
                    if (conv != null)
                        te.NullText = conv.ParamToFormat;
                    te.DisplayTextConverter = DisplayTextConverter;
                }
                if (!be.IsReadOnly)
                    be.IsReadOnly = IsReadOnlyRightDependcy;
                if (IsMergedProperty.HasValue)
                    be.Background = (IsReadOnly || IsMergedProperty.Value) ? _trueBackground : Brushes.Orange;
            }
        }

        private void BaseEditOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            var baseEdit = sender as BaseEdit;
            if (baseEdit == null)
                return;
            DispatcherHelper.BeginInvoke((Action)(baseEdit.SelectAll));
        }

        protected override bool GetIsActuallyReadOnly()
        {
            return IsReadOnly;
        }

        public void SetLabelProperties(ValueDataField field)
        {
            if (field == null)
                return;

            LabelPosition = field.LabelPosition.To(LayoutItemLabelPosition.Left);
            if (field.LabelPosition.EqIgnoreCase(ValueDataFieldConstants.None))
                LabelVisibility = Visibility.Collapsed;
        }

        public void Dispose()
        {
            if (PropertiesBinding != null)
            {
                PropertiesBinding.Clear();
                PropertiesBinding = null;
            }

            // прибиваем контент
            var disp = Content as IDisposable;
            if (disp != null)
                disp.Dispose();
        }

        public static void WritePropertyToXML(DependencyObject o, XmlWriter xml, DependencyProperty property, string propertyName)
        {
            if (o.IsPropertyAssigned(property))
            {
                string name;
                var obj = o.GetValue(property);
                if (obj == null)
                {
                    name = null;
                }
                else if (obj is double)
                {
                    name = ((double)obj).ToString(CultureInfo.InvariantCulture);
                }
                else if (obj is Thickness)
                {
                    name = ThicknessHelper.ToString((Thickness)obj);
                }
                else if (obj is FrameworkElement)
                {
                    name = ((FrameworkElement)obj).Name;
                }
                else
                {
                    name = obj.ToString();
                }
                xml.WriteAttributeString(propertyName, name);
            }
        }

        #endregion .  Methods  .
    }
}