using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Filtering;
using DevExpress.Xpf.Editors.Settings;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Main.Views
{
    public partial class FilterView : CustomUserControl, IHelpHandler, IPanelView
    {
        private bool _isInitialized;
        private readonly string _validError = StringResources.ValidErrorFilterRowCount;
        private IFilterViewModel _vm;

        public FilterView()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;
            DataContextChanged += OnDataContextChanged;
            TextRowCount.EditValueChanged += ValueChangedRowCount;

            var dc = DataContext as PanelViewModelBase;
            if (dc != null)
                PanelCaption = dc.PanelCaption;
        }

        private IFilterViewModel GetFilterViewModel()
        {
            return _vm ?? (_vm = DataContext as IFilterViewModel);
        }

        private void ValueChangedRowCount(object sender, EditValueChangedEventArgs e)
        {
            DispatcherHelper.BeginInvoke((ThreadStart)(() => TextRowCount.Focus()));
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            FillTree();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var fvm = GetFilterViewModel();
            if (fvm != null)
            {
                fvm.FixFilterExpressionRequest -= OnFixFilterExpressionRequest;
                fvm.FixFilterExpressionRequest += OnFixFilterExpressionRequest;
                // фильтр мог быть выставлен заранее
                if (string.IsNullOrEmpty(fvm.FilterExpression))
                    fvm.ToDefault();
            }
        }

        private void OnFixFilterExpressionRequest(object sender, EventArgs e)
        {
            FixFilterExpression();
        }

        private void FixFilterExpression()
        {
            //Значение vm.FilterExpression может обнуляться в глубинах контрола при вызове метода ApplyFilter. Данный код не влияет на изменение значения vm.FilterExpression вне контрола.
            var vm = GetFilterViewModel();
            string filterExpression = null;
            if (vm != null)
                filterExpression = vm.FilterExpression;

            ApplyFilter();
            
            if (vm != null)
            {
                if (filterExpression != null && vm.FilterExpression == null)
                    vm.FilterExpression = filterExpression;

                vm.AcceptChanges();
            }
        }

        private void ApplyFilter()
        {
            baseFilter.Dispatcher.Invoke((Action)baseFilter.ApplyFilter);
        }

        private void FillTree()
        {
            var fvm = GetFilterViewModel();
            if (fvm == null)
                return;

            var columns = new List<FilterColumn>();
            foreach (var field in fvm.Fields)
            {
                var column = new FilterColumn
                {
                    ColumnCaption = field.Caption,
                    ColumnType = field.FieldType,
                    FieldName = field.SourceName,
                    EditSettings = CreateEditorSettings(field)
                };
                columns.Add(column);
            }
            baseFilter.FilterColumns = columns;

            // таким хитрым способом заставляем перерисоваться фильтр
            var prev = fvm.FilterExpression;
            fvm.FilterExpression = null;
            fvm.FilterExpression = prev;
        }

        private static BaseEditSettings CreateEditorSettings(DataField field)
        {
            var flag = field.FieldType.IsNullable();
            var trueType = field.FieldType.GetNonNullableType();

            BaseEditSettings settings;

            if (!string.IsNullOrEmpty(field.LookupCode))
            {
                settings = new CustomLookUpEditSettings {LookUpCodeEditor = field.LookupCode, ImmediatePopup = true };
            }
            else if (trueType == typeof (DateTime))
                settings = new DateEditSettings
                {
                    MaskUseAsDisplayFormat = true,
                    AllowNullInput = flag,
                    Mask = field.DisplayFormat
                };
            else if (trueType == typeof (Boolean))
                settings = new CheckEditSettings {IsThreeState = flag};
            else
            {
                IValueConverter valueConverter;
                settings = DataField.TryGetFieldProperties(field, ValueDataFieldConstants.BindingIValueConverter, false, out valueConverter) 
                    ? new CustomTextEditSettings() { Converter = valueConverter, ValidateOnTextInput = false } 
                    : new TextEditSettings();
            }

            settings.DisplayFormat = field.DisplayFormat;
            settings.DisplayTextConverter = field.DisplayTextConverter;
            return settings;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            FocusOnFilter(false);
            FixFilterExpression();

            var fvm = GetFilterViewModel();
            if (fvm == null)
                return;

            // применяем фильтр
            if (fvm.ApplyFilterCommand != null)
                fvm.ApplyFilterCommand.Execute(null);
        }

        private void OnRemoveBlank(object sender, ItemClickEventArgs e)
        {
            FocusOnFilter();
            baseFilter.FilterCriteria = FilterHelperEx.RemoveCriteriaWithNotSetValue(baseFilter.FilterCriteria);
        }

        private void FocusOnFilter(bool isAppliedFilter = true)
        {
            var request = new TraversalRequest(new FocusNavigationDirection());
            baseFilter.MoveFocus(request);
            if (isAppliedFilter)
                FixFilterExpression();
        }

        #region . IHelpHandler .
        string IHelpHandler.GetHelpLink()
        {
            return "Filter";
        }

        string IHelpHandler.GetHelpEntity()
        {
            return null;
        }
        #endregion

        #region . IDisposable .
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var fvm = GetFilterViewModel();
                // events
                if (fvm != null)
                {
                    fvm.FixFilterExpressionRequest -= OnFixFilterExpressionRequest;
                    fvm.Dispose();
                }

                // убиваем фильтр
                if (baseFilter != null && baseFilter.FilterColumns != null)
                {
                    foreach (var filterColumn in baseFilter.FilterColumns)
                    {
                        var dispSettings = filterColumn.EditSettings as IDisposable;
                        if (dispSettings != null)
                            dispSettings.Dispose();
                    }
                }

                IsVisibleChanged -= OnIsVisibleChanged;
                DataContextChanged -= OnDataContextChanged;
                TextRowCount.EditValueChanged -= ValueChangedRowCount;
            }

            base.Dispose(disposing);
        }
        #endregion

        # region IPanelView

        public bool CanClose()
        {
            if (_vm == null || _vm.IsFilterMode)
                return true;

            //Обновляем фильтр при закрытии формы в режиме IsFilterMode = false
            FocusOnFilter();
            return true;
        }

        public string PanelCaption { get; set; }

        # endregion IPanelView

        private void TextRowCount_OnValidate(object sender, ValidationEventArgs e)
        {
            var isValid = false;
            if (e.Value == null || string.IsNullOrEmpty(e.Value.ToString()))
                isValid = true;
            else
            {
                Decimal newValue;
                if (Decimal.TryParse(e.Value.ToString(), out newValue))
                    if (newValue <= 0)
                        isValid = true;
            }
            if (!isValid) 
                return;

            e.IsValid = false;
            e.ErrorType = DevExpress.XtraEditors.DXErrorProvider.ErrorType.Critical;
            e.ErrorContent = _validError;
        }

        private void OnFilterItemClick(object sender, ItemClickEventArgs e)
        {
            FocusOnFilter();
        }

        private void OnFilterCancelClick(object sender, ItemClickEventArgs e)
        {
            FocusOnFilter(false);
            ApplyFilter();
        }
    }
}
