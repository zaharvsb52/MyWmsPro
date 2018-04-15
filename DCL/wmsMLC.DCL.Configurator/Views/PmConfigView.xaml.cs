using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.Utils;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Configurator.ViewModels;
using wmsMLC.DCL.Configurator.Views.Controls;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General;
using wmsMLC.General.BL;
using ColumnFilterMode = DevExpress.Xpf.Grid.ColumnFilterMode;

namespace wmsMLC.DCL.Configurator.Views
{
    //RestoreLayoutFromXml does not work properly when it contains bands - http://www.devexpress.com/Support/Center/Question/Details/B238182
    //DataTemplateSelector - http://www.devexpress.com/Support/Center/Question/Details/Q485124
    public partial class PmConfigView
    {
        private const string ColUnboundMethodsFieldName = "ColUnboundMethods";
        public const string ChangedColumnTemplateKey = "ChangedColumnTemplate";
        public const string NotAllowedPmMethodCellTemplateKey = "NotAllowedPmMethodCellTemplate";

        private PmConfigViewModel _vm;

        public PmConfigView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            PmGridControl.Loaded += OnGridLoaded;
            PmGridControl.RestoredLayoutFromXml += OnGridRestoredLayoutFromXml;
            PmGridControl.SelectionChanged += OnGridSelectionChanged;
            PmGridControl.ItemsSourceChanged += OnGridItemsSourceChanged;
        }

        protected override void OnClose()
        {
            DataContextChanged -= OnDataContextChanged;
            PmGridControl.Loaded -= OnGridLoaded;
            PmGridControl.RestoredLayoutFromXml -= OnGridRestoredLayoutFromXml;
            PmGridControl.SelectionChanged -= OnGridSelectionChanged;
            PmGridControl.ItemsSourceChanged -= OnGridItemsSourceChanged;

            base.OnClose();
            if (_vm != null)
            {
                _vm.Dispose();
                _vm = null;
            }
        }

        private void OnGridLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            PmGridControl.Loaded -= OnGridLoaded;
            RestoreColumnsSettings();
        }

        //private void OnGridControlPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName != "VisibleRowCount")
        //        return;

        //    if (PmGridControl.VisibleRowCount > 0)
        //    {
        //        //Операции
        //        foreach (var col in PmGridControl.Columns.Where(p => Equals(p.Tag, ColUnboundMethodsFieldName)))
        //        {
        //            PmGridControl.View.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
        //                new Action(() => ((TableView) PmGridControl.View).BestFitColumn(col)));
        //        }
        //    }
        //}

        private void OnGridRestoredLayoutFromXml(object sender, EventArgs eventArgs)
        {
            PmGridControl.RestoredLayoutFromXml -= OnGridRestoredLayoutFromXml;
            RestoreColumnsSettings();
        }

        private void RestoreColumnsSettings()
        {
            //Восстанавливаем Settings
            try
            {
                //Картинка
                PmGridControl.BeginDataUpdate();
                var column = PmGridControl.Columns.Single(p => p.FieldName == "IsDirty");
                column.DisplayTemplate = (ControlTemplate) Resources[ChangedColumnTemplateKey];
                column.EditSettings = new ImageEditSettings
                {
                    ShowMenu = false,
                };

                //PM
                column = PmGridControl.Columns.Single(p => p.FieldName == PmConfiguratorData.PmCodePropertyName);
                column.HeaderToolTipTemplate = CreateHeaderSuperTipTemplate(column.Header, null, column.FieldName);
                column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                column.EditSettings = new ConfiguratorLookUpEditSettings
                {
                    //LookUpCodeEditor = "PM_PMCODE",
                    ValueMember = new PM().GetPrimaryKeyPropertyName(),
                    DisplayMember = "PMNAME",
                    IsPopupAutoWidth = false,
                    AllowNullInput = true,
                    ShowSizeGrip = false,
                    IsTextEditable = false,
                    ItemType = typeof(PM)
                };
                var bindingPms = new Binding(PmConfigViewModel.PmsPropertyName)
                {
                    //IsAsync = true,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                BindingOperations.SetBinding(column.EditSettings, LookUpEditSettingsBase.ItemsSourceProperty, bindingPms);

                //Сущности
                column = PmGridControl.Columns.Single(p => p.FieldName == PmConfiguratorData.ObjectEntityCodePropertyName);
                column.HeaderToolTipTemplate = CreateHeaderSuperTipTemplate(column.Header, null, column.FieldName);
                column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                column.EditSettings = new ConfiguratorComboBoxEditSettings
                {
                    LookUpCodeEditor = "SYSOBJECT_ENTITY",
                    //ValueMember = "OBJECTNAME",
                    //DisplayMember = "OBJECTNAME",
                    AllowNullInput = true,
                    ShowSizeGrip = false,
                    IsTextEditable = false,
                    AllowParentGridRefreshData = true
                };

                //Атрибуты
                column = PmGridControl.Columns.Single(p => p.FieldName == PmConfiguratorData.ObjectNamePropertyName);
                column.HeaderToolTipTemplate = CreateHeaderSuperTipTemplate(column.Header, null, column.FieldName);
                column.ColumnFilterMode = ColumnFilterMode.DisplayText;
                column.EditSettings = new ConfiguratorComboBoxEditSettings
                {
                    LookUpCodeEditor = "SYSOBJECT_ATTRIBUTE",
                    LookUpCodeEditorFilterExt = "OBJECTPARENTID > 0",
                    //ValueMember = "OBJECTNAME",
                    //DisplayMember = "OBJECTNAME",
                    AllowNullInput = true,
                    ShowSizeGrip = false,
                    IsTextEditable = false,
                    AllowParentGridRefreshData = true
                };

                //Операции
                var notAllowedPmMethodCellTemplate = TryFindResource(NotAllowedPmMethodCellTemplateKey) as DataTemplate;
                var columnHeaderMethodDescription = Properties.Resources.ColumnHeaderMethodDescription;
                var pmMethodValue = new PMMethod().GetPrimaryKeyPropertyName();
                foreach (var col in PmGridControl.Columns.Where(p => Equals(p.Tag, ColUnboundMethodsFieldName)))
                {
                    //Не разрешаем редактировать
                    col.AllowEditing = DefaultBoolean.False;
                    col.ReadOnly = true;

                    col.HeaderToolTipTemplate = CreateHeaderSuperTipTemplate(col.Header, string.Format(columnHeaderMethodDescription, col.Header), col.FieldName);
                    col.CellTemplateSelector = new PmMethodCellTemplateSelector
                    {
                        NotAllowedPmMethodsCellTemplate = notAllowedPmMethodCellTemplate
                    };
                    col.EditSettings = new ConfiguratorComboBoxEditSettings
                    {
                        //LookUpCodeEditor = "PMMETHOD_PMMETHODCODE",
                        ValueMember = pmMethodValue,
                        DisplayMember = PMMethod.PMMETHODNAMEPropertyName,
                        AllowNullInput = true,
                        ShowSizeGrip = false,
                        IsTextEditable = false,
                        //ShowTooltipForTrimmedText = true,
                        SeparatorString = Environment.NewLine,
                        StyleSettings = new CheckedComboBoxStyleSettings()
                        //InsertedItems = new object[] {Properties.Resources.PmMethodIsUnavailable}
                    };
                    var binding = new Binding(PmConfigViewModel.PmMethodsPropertyName)
                    {
                        //IsAsync = true,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding(col.EditSettings, LookUpEditSettingsBase.ItemsSourceProperty, binding);
                }
            }
            finally
            {
                PmGridControl.EndDataUpdate();
            }
        }

        private void OnNewRowAdded(object sender, EventArgs eventArgs)
        {
            try
            {
                PmGridControl.BeginSelection();
                var index = PmGridControl.FindRowByValue(PmConfiguratorData.IsNewRowPropertyName, true);
                if (index >= 0)
                {
                    PmGridControl.UnselectAll();
                    PmGridControl.SelectItem(index);
                    PmGridControl.CurrentColumn = PmGridControl.Columns[PmConfiguratorData.PmCodePropertyName];
                    PmGridControl.View.MoveFocusedRow(index);
                    PmGridControl.View.FocusedRowHandle = index;
                    PmGridControl.View.ShowEditor();
                }
            }
            finally
            {
                PmGridControl.EndSelection();
            }
        }

        private void OnBeforeSave(object sender, EventArgs eventArgs)
        {
            EndEdit();
        }

        private void OnBeforeRefresh(object sender, EventArgs eventArgs)
        {
            MethodView.DataContext = null;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm == null)
                _vm = DataContext as PmConfigViewModel;
            if (_vm == null || PmGridControl.Columns.Any(p => Equals(p.Tag, ColUnboundMethodsFieldName)))
                return;

            try
            {
                PmGridControl.BeginDataUpdate();

                foreach (var op in _vm.GetOperation())
                {
                    var opcode = op.GetProperty<string>(BillOperation.OperationCodePropertyName);
                    var header = op.GetProperty<string>(BillOperation.OperationNamePropertyName);

                    var column = new GridColumn
                    {
                        Header = header,
                        FieldName = opcode,
                        Width = 200,
                        AllowUnboundExpressionEditor = false,
                        UnboundType = UnboundColumnType.Object,
                        AllowSorting = DefaultBoolean.True,
                        SortMode = ColumnSortMode.DisplayText,
                        //AllowAutoFilter = false,
                        //AllowColumnFiltering  = DefaultBoolean.False,
                        //AutoFilterCondition = new AutoFilterCondition(),
                        FilterPopupMode = FilterPopupMode.CheckedList,
                        ColumnFilterMode = ColumnFilterMode.DisplayText,
                        Tag = ColUnboundMethodsFieldName
                    };

                    PmGridControl.Columns.Add(column);
                }

                _vm.NewRowAdded -= OnNewRowAdded;
                _vm.NewRowAdded += OnNewRowAdded;

                _vm.BeforeSave -= OnBeforeSave;
                _vm.BeforeSave += OnBeforeSave;

                _vm.BeforeRefresh -= OnBeforeRefresh;
                _vm.BeforeRefresh += OnBeforeRefresh;
            }
            finally
            {
                PmGridControl.EndDataUpdate();
            }
        }

        private DataTemplate CreateHeaderSuperTipTemplate(object header, string description, string field)
        {
            if (header == null)
                throw new ArgumentNullException("header");

            var xaml = new StringBuilder();
            xaml.AppendLine("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            xaml.AppendLine("xmlns:dx=\"http://schemas.devexpress.com/winfx/2008/xaml/core\">");
            xaml.AppendLine("<dx:SuperTipControl>");
            xaml.AppendLine("<dx:SuperTip>");
            xaml.AppendLine(string.Format("<dx:SuperTipItem Content=\"{0}\"/>", header));

            if (!string.IsNullOrEmpty(description))
            {
                xaml.AppendLine("<dx:SuperTipItemSeparator/>");
                xaml.AppendLine(string.Format("<dx:SuperTipItem Content=\"{0}\"/>", description));
            }

            if (!string.IsNullOrEmpty(field))
            {
                xaml.AppendLine("<dx:SuperTipItemSeparator/>");
                xaml.AppendLine(string.Format("<dx:SuperTipItem Content=\"{0}\"/>", field));
            }

            xaml.AppendLine("</dx:SuperTip>");
            xaml.AppendLine("</dx:SuperTipControl>");
            xaml.AppendLine("</DataTemplate>");
            var dataTemplate = (DataTemplate)XamlReader.Parse(xaml.ToString());
            return dataTemplate;
        }

        private void OnCustomUnboundColumnData(object sender, GridColumnDataEventArgs e)
        {
            if (Equals(e.Column.Tag, ColUnboundMethodsFieldName))
            {
                var key = e.Column.FieldName;
                var row = PmGridControl.GetRowByListIndex(e.ListSourceRowIndex) as PmConfiguratorData;
                var isvalid = _vm != null && row != null && _vm.ValidatePmMethods(key, row);
                var methods = (IDictionary<string, EditableBusinessObjectCollection<object>>)
                    e.GetListSourceFieldValue(PmConfiguratorData.PmMethodCodesPropertyName);

                if (e.IsGetData)
                {
                    var values = GetTrueMethods(key, methods).ToList();
                    if (!isvalid)
                        values.Insert(0, Properties.Resources.PmMethodIsUnavailable);
                    e.Value = values;
                }

                if (e.IsSetData && isvalid)
                {
                    var pms = GetTrueMethods(key, methods);
                    pms.Clear();
                    if (e.Value != null)
                    {
                        foreach (var p in (List<object>) e.Value)
                        {
                            pms.Add(p);
                        }
                    }

                    row.PmMethodCodesPropertyChanged();
                }
            }
        }

        private void OnShowingEditor(object sender, ShowingEditorEventArgs e)
        {
            if (e.RowHandle == DataControlBase.AutoFilterRowHandle)
                return;

            //http://documentation.devexpress.com/#WPF/CustomDocument6135
            CriteriaOperator co = null;
            var hascriteria = false;

            try
            {
                PmGridControl.BeginDataUpdate();

                var row = e.Row as PmConfiguratorData;
                if (row == null)
                {
                    e.Cancel = true;
                    return;
                }

                var column = e.Column;
                if (column.FieldName == PmConfiguratorData.ObjectEntityCodePropertyName)
                {
                    if (string.IsNullOrEmpty(row.PmCode))
                    {
                        e.Cancel = true;
                        return;
                    }

                    //Если сделать в Loaded не работает клизма
                    var settings = column.EditSettings as IConfiguratorSettingsBase;
                    if (settings != null)
                    {
                        hascriteria = true;
                        var pmkeys = _vm.GetEntityIds();
                        foreach (var p in pmkeys)
                        {
                            co |= new BinaryOperator(SysObject.ObjectIDPropertyName, p, BinaryOperatorType.Equal);
                        }
                        settings.LookupFilterCriteria = co;
                    }
                }
                else if (column.FieldName == PmConfiguratorData.ObjectNamePropertyName)
                {
                    if (string.IsNullOrEmpty(row.PmCode) || string.IsNullOrEmpty(row.OjectEntityCode))
                    {
                        e.Cancel = true;
                        return;
                    }

                    var settings = column.EditSettings as IConfiguratorSettingsBase;
                    if (settings != null)
                    {
                        hascriteria = true;
                        var pmkeys = _vm.GetAttributes(row.OjectEntityCode);
                        
                        //CriteriaOperator co = new BinaryOperator(SysObject.ObjectIDPropertyName, (object)null, BinaryOperatorType.Equal);
                        foreach (var p in pmkeys)
                        {
                            co |= new BinaryOperator(SysObject.ObjectIDPropertyName, p, BinaryOperatorType.Equal);
                        }

                        settings.LookupFilterCriteria = co;
                    }
                }
                else if (Equals(column.Tag, ColUnboundMethodsFieldName)) //Методы
                {
                    //Редактируем в другой форме
                    e.Cancel = true;

                    //if (string.IsNullOrEmpty(column.FieldName) ||
                    //    string.IsNullOrEmpty(row.PmCode) || string.IsNullOrEmpty(row.OjectEntityCode) || string.IsNullOrEmpty(row.ObjectName))
                    //{
                    //    e.Cancel = true;
                    //    return;
                    //}

                    //var settings = column.EditSettings as IConfiguratorSettingsBase;
                    //if (settings != null)
                    //{
                    //    hascriteria = true;
                    //    var pmkeys = _vm.GetAllowedPmMethods(operationCode: column.FieldName,
                    //        objectEntityCode: row.OjectEntityCode, objectName: row.ObjectName);

                    //    var pms =
                    //        _vm.ItemsSource.Where(
                    //            p => p.PmMethodCodes != null && p.Id != row.Id &&
                    //                p.PmCode == row.PmCode && p.OjectEntityCode == row.OjectEntityCode &&
                    //                p.ObjectName == row.ObjectName)
                    //            .SelectMany(p => p.PmMethodCodes.Values)
                    //            .SelectMany(m => m.Cast<string>())
                    //            .Distinct().ToArray();

                    //    foreach (var p in pmkeys)
                    //    {
                    //        if (pms.Any(pm => pm.EqIgnoreCase(p)))
                    //            continue;
                    //        co |= new BinaryOperator("PMMETHODCODE", p, BinaryOperatorType.Equal);
                    //    }

                    //    settings.LookupFilterCriteria = co;
                    //}
                }
            }
            finally
            {
                if (hascriteria)
                {
                    if (ReferenceEquals(co, null))
                        e.Cancel = true;
                }
                PmGridControl.EndDataUpdate();
            }
        }

        private void OnCellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            var column = e.Column;
            var row = e.Row as PmConfiguratorData;
            if (column == null || row == null || _vm == null)
                return;

            Action<bool> handlerDataForDelete = useOldValue =>
            {
                if (!_vm.DataForDelete.ContainsKey(row.Id))
                {
                    _vm.DataForDelete[row.Id] = row.Clone();
                    if (useOldValue)
                    {
                        var oldvalues = e.OldValue as List<object>;
                        var oldvalue = oldvalues == null
                            ? new EditableBusinessObjectCollection<object>()
                            : new EditableBusinessObjectCollection<object>(oldvalues);
                        oldvalue.AcceptChanges();
                        _vm.DataForDelete[row.Id].PmMethodCodes[column.FieldName] = oldvalue;
                    }
                }
            };
            
            switch (column.FieldName)
            {
                case PmConfiguratorData.PmCodePropertyName: //Очищаем поля справа
                    handlerDataForDelete(false);
                    row.PmCode = e.Value.To<string>();
                    break;
                case PmConfiguratorData.ObjectEntityCodePropertyName: //Очищаем поля справа
                    handlerDataForDelete(false);
                    row.OjectEntityCode = e.Value.To<string>();
                    break;
                case PmConfiguratorData.ObjectNamePropertyName: //Очищаем поля справа
                    handlerDataForDelete(false);
                    row.ObjectName = e.Value.To<string>();
                    break;
                default:
                    if (Equals(column.Tag, ColUnboundMethodsFieldName)) //Методы
                        handlerDataForDelete(true); //Подготавливаем удаление PmConfig
                    break;
            }

            if (_vm != null)
                _vm.RiseCommandsCanExecute();
        }

        private EditableBusinessObjectCollection<object> GetTrueMethods(string opcode, IDictionary<string, EditableBusinessObjectCollection<object>> methods)
        {
            if (methods == null)
                throw new ArgumentNullException("methods");

            var emptyResult = new EditableBusinessObjectCollection<object>();
            emptyResult.AcceptChanges();

            if (string.IsNullOrEmpty(opcode))
                return emptyResult;

            if (!methods.ContainsKey(opcode))
                methods[opcode] = emptyResult;

            return methods[opcode] ?? (methods[opcode] = emptyResult);
        }

        private void OnMenuPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndEdit();
        }

        private void EndEdit()
        {
            PmGridControl.View.CommitEditing();
        }

        private void OnGridSelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            if (_vm == null)
                return;

            var grid = sender as GridControl;
            if (grid == null)
                return;

            var selectedItem = grid.GetFocusedRow() as PmConfiguratorData ?? new PmConfiguratorData(_vm);
            var vm = selectedItem.MethodViewModel ?? new PmMethodViewModel(_vm);

            try
            {
                vm.NotNeedRefresh = true;
                vm.SelectedItem = selectedItem;
                vm.PmName = grid.GetCellDisplayText(grid.View.FocusedRowHandle,
                    grid.Columns[PmConfiguratorData.PmCodePropertyName]);

                bool? isvalid = null;
                object opearatonName = null;
                string operationCode = null;
                List<object> selectedMethods = null;
                List<PmMethodViewModel.CheckedPmMethod> itemsSource = null;
                var currentColumn = grid.CurrentColumn as GridColumn;
                if (currentColumn != null)
                {
                    if (Equals(currentColumn.Tag, ColUnboundMethodsFieldName))
                    {
                        opearatonName = currentColumn.Header;
                        operationCode = currentColumn.FieldName;

                        if (!string.IsNullOrEmpty(operationCode))
                        {
                            isvalid = _vm != null && _vm.ValidatePmMethods(operationCode, selectedItem);
                            if (isvalid == true)
                            {
                                var pmkeys = _vm.GetAllowedPmMethods(operationCode: operationCode,
                                    objectEntityCode: selectedItem.OjectEntityCode, objectName: selectedItem.ObjectName);
                                itemsSource = _vm.PmMethods
                                    .Where(p => pmkeys.Contains(p.GetKey<string>()))
                                    .Select(p => new PmMethodViewModel.CheckedPmMethod(vm)
                                         {
                                             Method = p
                                         })
                                         .ToList();
                                selectedMethods = GetTrueMethods(operationCode, selectedItem.PmMethodCodes).ToList();
                            }
                        }
                    }
                }

                vm.OperationName = opearatonName;
                vm.OperationCode = operationCode;
                vm.IsValid = isvalid;
                vm.SelectedMethods = selectedMethods;
                vm.ItemsSource = itemsSource;
                vm.SetSelectionMetods();
                if (!ReferenceEquals(MethodView.DataContext, vm))
                {
                    MethodView.DataContextChanged -= OnMethodViewDataContextChanged;
                    MethodView.DataContextChanged += OnMethodViewDataContextChanged;
                    MethodView.DataContext = vm;
                }
            }
            finally
            {
                vm.NotNeedRefresh = false;
            }
        }

        private void OnMethodViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MethodView.DataContextChanged -= OnMethodViewDataContextChanged;
            var vm = e.OldValue as PmMethodViewModel;
            if (vm != null)
            {
                vm.OperationCode = null;
                vm.NotNeedRefresh = true;
            }
        }

        private void OnGridItemsSourceChanged(object sender, ItemsSourceChangedEventArgs e)
        {
            if (e.NewItemsSource == null)
                return;

            var grid = sender as CustomGridControl;
            if (grid == null)
                return;

            var selectionItems = grid.SelectedItems;
            if (selectionItems == null || selectionItems.Count == 0)
            {
                grid.SelectItem(0);
                return;
            }

            var itemsSource = grid.ItemsSource as IList;
            if (itemsSource == null)
                return;

            var tableView = grid.View as TableView;
            if (tableView == null)
                return;

            try
            {
                grid.BeginSelection();
                grid.UnselectAll();

                foreach (var item in selectionItems.OfType<PmConfiguratorData>())
                {
                    var ikh = item as IKeyHandler;
                    if (ikh == null)
                        continue;

                    var index = grid.IndexOf(ikh.GetKey());
                    if (index < 0)
                        continue;

                    grid.SelectItem(index);
                    MoveFocusedRow(tableView, index);
                }
            }
            finally
            {
                grid.EndSelection();
            }
        }

        private void MoveFocusedRow(DataViewBase tableView, int index)
        {
            if (tableView == null)
                return;

            tableView.MoveFocusedRow(index);
            tableView.FocusedRowHandle = index;
        }

        private void OnGridCustomRowFilter(object sender, RowFilterEventArgs e)
        {
            var row = PmGridControl.GetRowByListIndex(e.ListSourceRowIndex) as PmConfiguratorData;
            if (row == null)
                return;

            //Не фильтруем новые записи
            if (row.IsNew)
            {
                e.Visible = true;
                e.Handled = true;
            }
        }

        private void OnGridContextMenuItemClick(object sender, ItemClickEventArgs e)
        {
            var oldMode = PmGridControl.ClipboardCopyMode;
            try
            {
                var mode = (ClipboardCopyMode)Enum.Parse(typeof(ClipboardCopyMode), e.Item.CommandParameter.ToString());
                PmGridControl.ClipboardCopyMode = mode;
                PmGridControl.ByCheckCopyMode = false;
                PmGridControl.CopyToClipboard();
            }
            finally
            {
                PmGridControl.ClipboardCopyMode = oldMode;
                PmGridControl.ByCheckCopyMode = true;
            }
        }

        //private bool ValidateDotNet45FromRegistry()
        //{
        //    try
        //    {
        //        using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
        //            RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
        //        {
        //            //if (releaseKey == 378389)
        //            //Console.WriteLine("The .NET Framework version 4.5 is installed");
        //            //if (releaseKey == 378758)
        //            //Console.WriteLine("The .NET Framework version 4.5.1  is installed");
        //            if (ndpKey == null)
        //                return false;
        //            var releaseKey = (int) ndpKey.GetValue("Release");
        //            return releaseKey >= 378389;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.WarnFormat("Error in ValidateDotNet45FromRegistry {0}", ExceptionPolicy.ExceptionToString(ex));
        //        _log.Debug(ex);
        //    }
        //    return false;
        //}
    }
}
