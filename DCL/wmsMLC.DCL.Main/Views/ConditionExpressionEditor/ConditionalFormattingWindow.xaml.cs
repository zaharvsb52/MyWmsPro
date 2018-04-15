using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.ConditionalFormatting.Themes;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Themes;
using DevExpress.Xpf.Grid;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.DCL.Main.Views.ConditionExpressionEditor
{
    /// <summary>
    /// http://www.devexpress.com/Support/Center/Example/Details/E4256
    /// http://www.devexpress.com/Support/Center/Example/Details/E4272
    /// </summary>
    public partial class ConditionalFormattingWindow : INotifyPropertyChanged
    {
        private const string SelectedItemPropertyName = "SelectedItem";
        private ConditionExpressionEditorControl _expressionEditor;
        private bool _useOldConditionalFormatting;

        public ConditionalFormattingWindow(GridDataViewBase view, StyleOptionCollection opsCollection, bool useOldConditionalFormatting = false)
        {
            View = view;
            _useOldConditionalFormatting = useOldConditionalFormatting;
            InitializeComponent();
            ThresholdVisibility = Visibility.Collapsed;
            LayoutItemVisibility = Visibility.Visible;
            ApplyToRowVisibility = Visibility.Visible;
            PredefinedFormatNameVisibility = Visibility.Collapsed;
            PrepareCollection(opsCollection);
            InternalEditorsInit();
            ApplyButtonsThemes();

            Loaded += OnLoaded;
            Closed += OnClosed;
            LayoutRoot.DataContext = this;
        }

        #region . Properties .
        private StyleOptionCollection _optionsCollection;
        public StyleOptionCollection OptionsCollection
        {
            get { return _optionsCollection; }
            set
            {
                if (_optionsCollection == value) 
                    return;
                _optionsCollection = value;
                OnPropertyChanged("OptionsCollection");
            }
        }

        private StyleOption _selectedItem;
        public StyleOption SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem == value) 
                    return;
                _selectedItem = value;
                OnPropertyChanged(SelectedItemPropertyName);
                if (_selectedItem != null)
                {
                    _expressionEditor.SetExpression(IsNewExpressionString() ? null : _selectedItem.ExpressionString);
                    OnConditionFormatTypeChamged(_selectedItem.FormatConditionType);
                }
            }
        }

        private bool _waitIndicatorVisible;
        public bool WaitIndicatorVisible
        {
            get { return _waitIndicatorVisible; }
            set
            {
                if (_waitIndicatorVisible == value)
                    return;
                _waitIndicatorVisible = value;
                OnPropertyChanged("WaitIndicatorVisible");
            }
        }

        private bool _isDeleteButtonEnabled;

        public bool IsDeleteButtonEnabled
        {
            get { return _isDeleteButtonEnabled; }
            set
            {
                if (_isDeleteButtonEnabled == value)
                    return;
                _isDeleteButtonEnabled = value;
                OnPropertyChanged("IsDeleteButtonEnabled");
            }
        }

        private List<ConditionalFormattingType> _conditionalFormattingTypes;
        public List<ConditionalFormattingType> ConditionalFormattingTypes
        {
            get
            {
                return _conditionalFormattingTypes ??
                       (_conditionalFormattingTypes = new List<ConditionalFormattingType>());
            }
            set
            {
                if (_conditionalFormattingTypes == value)
                    return;
                _conditionalFormattingTypes = value;
                OnPropertyChanged("ConditionalFormattingTypes");
            }
        }

        private string _thresholdLabel;
        public string ThresholdLabel
        {
            get { return _thresholdLabel; }
            set
            {
                if (_thresholdLabel == value)
                    return;
                _thresholdLabel = value;
                OnPropertyChanged("ThresholdLabel");
            }
        }

        private Visibility _thresholdVisibility;
        public Visibility ThresholdVisibility
        {
            get { return _thresholdVisibility; }
            set
            {
                if (_thresholdVisibility == value)
                    return;
                _thresholdVisibility = value;
                OnPropertyChanged("ThresholdVisibility");
            }
        }

        private Visibility _layoutItemVisibility;
        public Visibility LayoutItemVisibility
        {
            get { return _layoutItemVisibility; }
            set
            {
                if (_layoutItemVisibility == value)
                    return;
                _layoutItemVisibility = value;
                OnPropertyChanged("LayoutItemVisibility");
            }
        }

        private Visibility _applyToRowVisibility;
        public Visibility ApplyToRowVisibility
        {
            get { return _applyToRowVisibility; }
            set
            {
                if (_applyToRowVisibility == value)
                    return;
                _applyToRowVisibility = value;
                OnPropertyChanged("ApplyToRowVisibility");
            }
        }

        private Visibility _predefinedFormatNameVisibility;
        public Visibility PredefinedFormatNameVisibility
        {
            get { return _predefinedFormatNameVisibility; }
            set
            {
                if (_predefinedFormatNameVisibility == value)
                    return;
                _predefinedFormatNameVisibility = value;
                OnPropertyChanged("PredefinedFormatNameVisibility");
            }
        }

        private ConditionalFormattingThemeKeys _formatTemplateKey;
        public ConditionalFormattingThemeKeys FormatTemplateKey
        {
            get { return _formatTemplateKey; }
            set
            {
                if(_formatTemplateKey == value)
                    return;
                _formatTemplateKey = value;
                OnPropertyChanged("FormatTemplateKey");
            }
        }

        public GridDataViewBase View { get; set; }
        private StyleOptionCollection PrimaryStyleCollection { get; set; }

        public string NewExpressionString
        {
            get { return StringResources.StyleOptionNewExpressionString; }
        }

        public object ViewColumns
        {
            get
            {
                if (View == null)
                    return null;
                if (View.DataControl is GridControl)
                    return ((GridControl) View.DataControl).Columns;
                if (View.DataControl is TreeListControl)
                    return ((TreeListControl) View.DataControl).Columns;
                return null;
            }
        }
        #endregion . Properties .

        #region . Methods .
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            if (OptionsCollection.Count > 0)
                expressionsListBox.SelectedIndex = 0;

            ConditionalFormattingTypes = _useOldConditionalFormatting
                ? new List<ConditionalFormattingType>
                {
                    new ConditionalFormattingType
                    {
                        Id = FormatConditionType.Default,
                        Name = FormatConditionTypeEnumFormatting(FormatConditionType.Default)
                    }
                }
                : (from FormatConditionType formatConditionType in Enum.GetValues(typeof (FormatConditionType))
                    select new ConditionalFormattingType
                    {
                        Id = formatConditionType,
                        Name = FormatConditionTypeEnumFormatting(formatConditionType)
                    }).ToList();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Closed -= OnClosed;
            _expressionEditor = null;
        }

        private string FormatConditionTypeEnumFormatting(FormatConditionType formatConditionType)
        {
            switch (formatConditionType)
            {
                case FormatConditionType.Default:
                    return StringResources.FormatConditionTypeDefault;
                case FormatConditionType.TopItemsRule:
                    return StringResources.FormatConditionTypeTopItemsRule;
                case FormatConditionType.TopPersentRule:
                    return StringResources.FormatConditionTypeTopPersentRule;
                case FormatConditionType.BottomItemsRule:
                    return StringResources.FormatConditionTypeBottomItemsRule;
                case FormatConditionType.BottomPercentRule:
                    return StringResources.FormatConditionTypeBottomPercentRule;
                case FormatConditionType.AboveAverageRule:
                    return StringResources.FormatConditionTypeAboveAverageRule;
                case FormatConditionType.BelowAverageRule:
                    return StringResources.FormatConditionTypeBelowAverageRule;
                case FormatConditionType.DataBar:
                    return StringResources.FormatConditionTypeDataBar;
                case FormatConditionType.ColorScale:
                    return StringResources.FormatConditionTypeColorScale;
                case FormatConditionType.IconSet:
                    return StringResources.FormatConditionTypeIconSet;

                default:
                    throw new DeveloperException("Undefined FormatConditionType '{0}'.", formatConditionType);
            }
        }

        private void PrepareCollection(StyleOptionCollection opsCollection)
        {
            if (opsCollection != null)
            {
                PrimaryStyleCollection = opsCollection;
                OptionsCollection = opsCollection.Clone();
                OptionsCollection.ValidateHandler = expression => NewExpressionString.EqIgnoreCase(expression) ? StringResources.StyleOptionConditionNotDefined : null;
            }
        }

        private void ApplyButtonsThemes()
        {
            var addButtonTemplateRes = new TrackBarEditThemeKeyExtension
            {
                ResourceKey = TrackBarEditThemeKeys.RightStepButtonTemplate,
                ThemeName = ThemeManager.GetThemeName(this)
            };
            addButton.Template = (ControlTemplate)TryFindResource(addButtonTemplateRes);
            var deleteButtonTemplateRes = new TrackBarEditThemeKeyExtension
            {
                ResourceKey = TrackBarEditThemeKeys.LeftStepButtonTemplate,
                ThemeName = ThemeManager.GetThemeName(this)
            };
            deleteButton.Template = (ControlTemplate)TryFindResource(deleteButtonTemplateRes);
        }

        private void InternalEditorsInit()
        {
            _expressionEditor = new ConditionExpressionEditorControl(PrimaryStyleCollection.ColumnInfo);
            _expressionEditor.ExpressionChanged += delegate { UpdateExpressionString(); };

            var enabledBinding = new Binding(SelectedItemPropertyName)
            {
                Converter = new InternalStateConverter()
            };
            _expressionEditor.SetBinding(IsEnabledProperty, enabledBinding);
            expressionGroup.Children.Add(_expressionEditor);
        }

        private void UpdateExpressionString()
        {
            if (!IsSelectedItem() || (string.IsNullOrEmpty(_expressionEditor.ExpressionText) && IsNewExpressionString())) 
                return;
            SelectedItem.ExpressionString = _expressionEditor.ExpressionText;
        }

        private void OnOptionEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            SelectedItem = expressionsListBox.EditValue as StyleOption;
            ValidateDeleteButton();
        }

        private void OnConditionFormatTypeChamged(object sender, EditValueChangedEventArgs e)
        {
            var editValue = e.NewValue.To(FormatConditionType.Default);
            OnConditionFormatTypeChamged(editValue);
        }

        private void OnColumnChanged(object sender, EditValueChangedEventArgs e)
        {
            if (SelectedItem == null)
                return;
            OnConditionFormatTypeChamged(SelectedItem.FormatConditionType);
        }

        private void OnConditionFormatTypeChamged(FormatConditionType formatConditionType)
        {
            ThresholdVisibility = Visibility.Collapsed;
            LayoutItemVisibility = Visibility.Visible;
            ApplyToRowVisibility = Visibility.Visible;
            //_expressionEditor.IsEnabled = false;
            PredefinedFormatNameVisibility = Visibility.Collapsed;

            switch (formatConditionType)
            {
                case FormatConditionType.Default:
                    _expressionEditor.IsEnabled = true;
                    break;
                case FormatConditionType.TopItemsRule:
                    ThresholdLabel =
                        StringResources.ConditionalFormattingWindowAppearanceGroupThresholdLabel;
                    ThresholdVisibility = Visibility.Visible;
                    break;
                case FormatConditionType.TopPersentRule:
                    ThresholdLabel =
                        StringResources.ConditionalFormattingWindowAppearanceGroupThresholdInPercentLabel;
                    ThresholdVisibility = Visibility.Visible;
                    break;
                case FormatConditionType.BottomItemsRule:
                    ThresholdLabel =
                        StringResources.ConditionalFormattingWindowAppearanceGroupThresholdLabel;
                    ThresholdVisibility = Visibility.Visible;
                    break;
                case FormatConditionType.BottomPercentRule:
                    ThresholdLabel =
                        StringResources.ConditionalFormattingWindowAppearanceGroupThresholdInPercentLabel;
                    ThresholdVisibility = Visibility.Visible;
                    break;
                case FormatConditionType.AboveAverageRule:
                case FormatConditionType.BelowAverageRule:
                    //Не работает
                    ApplyToRowVisibility = Visibility.Collapsed;
                    break;
                case FormatConditionType.DataBar:
                    LayoutItemVisibility = Visibility.Collapsed;
                    ApplyToRowVisibility = Visibility.Collapsed;
                    PredefinedFormatNameVisibility = Visibility.Visible;
                    FormatTemplateKey = ConditionalFormattingThemeKeys.DataBarMenuItemContent;
                    break;
                case FormatConditionType.ColorScale:
                    LayoutItemVisibility = Visibility.Collapsed;
                    ApplyToRowVisibility = Visibility.Collapsed;
                    PredefinedFormatNameVisibility = Visibility.Visible;
                    FormatTemplateKey = ConditionalFormattingThemeKeys.ColorScaleMenuItemContent;
                    break;
                case FormatConditionType.IconSet:
                    LayoutItemVisibility = Visibility.Collapsed;
                    ApplyToRowVisibility = Visibility.Collapsed;
                    PredefinedFormatNameVisibility = Visibility.Visible;
                    FormatTemplateKey = ConditionalFormattingThemeKeys.IconSetMenuItemContent;
                    break;
            }
        }

        private void ValidateDeleteButton()
        {
            IsDeleteButtonEnabled = OptionsCollection != null && OptionsCollection.Any() && SelectedItem != null &&
                !SelectedItem.IsReadOnly;
        }

        private void OnAddOptionClick(object sender, RoutedEventArgs e)
        {
            var opt = new StyleOption
            {
                Name = NewExpressionString
            };
            OptionsCollection.Add(opt);
            expressionsListBox.SelectedIndex = OptionsCollection.Count - 1;
            expressionsListBox.ScrollIntoView(opt);
            ValidateDeleteButton();
        }

        private void OnRemoveOptionClick(object sender, RoutedEventArgs e)
        {
            ValidateDeleteButton();
            if (OptionsCollection.Any() && IsSelectedItem())
            {
                var index = expressionsListBox.SelectedIndex;
                OptionsCollection.RemoveAt(index);
                var count = OptionsCollection.Count;
                if (count > 0)
                {
                    if (index >= count)
                        index = count - 1;
                    expressionsListBox.SelectedIndex = index;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_expressionEditor.ExpressionText))
                        _expressionEditor.ExpressionText = string.Empty;
                }
            }
        }

        private async void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WaitIndicatorVisible = true;

                await DoEvent();

                var message = new List<string>();
                foreach (var op in OptionsCollection)
                {
                    var err = op.Validate();
                    if (err.Length > 0)
                    {
                        message.AddRange(
                            err.Where(m => !string.IsNullOrEmpty(m))
                                .Distinct()
                                .Select(
                                    m => string.Format("'{0}': {1}{2}", op.Name, m, Environment.NewLine)));
                    }
                }

                if (message.Count > 0)
                {
                    var viewService = IoC.Instance.Resolve<IViewService>();
                    viewService.ShowDialog(StringResources.Error
                        , string.Join(Environment.NewLine, message)
                        , MessageBoxButton.OK
                        , MessageBoxImage.Error
                        , MessageBoxResult.Yes);
                    return;
                }

                PrimaryStyleCollection.Clear();
                foreach (var t in OptionsCollection)
                {
                    PrimaryStyleCollection.Add(t);
                }

                DialogResult = true;
                Close();
            }
            finally
            {
                WaitIndicatorVisible = false;   
            }
        }

        private Task DoEvent()
        {
            return Task.Factory.StartNew(() => Thread.Sleep(300));
        }

        //Баг с AllowDrop
        private void OnPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var fe = (FrameworkElement)sender;
            var index = OptionsCollection.IndexOf((StyleOption)fe.DataContext);
            if (index >= 0)
                expressionsListBox.SelectedIndex = index;
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool IsSelectedItem()
        {
            return SelectedItem != null;
        }

        private bool IsNewExpressionString()
        {
            return IsSelectedItem() && SelectedItem.ExpressionString != null &&
                NewExpressionString.EqIgnoreCase(SelectedItem.ExpressionString);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        #endregion . Methods .

        public class ConditionalFormattingType
        {
            public FormatConditionType Id { get; set; }
            public string Name { get; set; }
        }
    }
}
