using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.Native;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Configurator.Helpers;
using wmsMLC.DCL.Configurator.Views;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Configurator.ViewModels
{
    [View(typeof(PmConfigView))]
    public class PmConfigViewModel : PanelViewModelBase, IPmConfigViewModel
    {
        public const string PmsPropertyName = "Pms";
        public const string PmMethodsPropertyName = "PmMethods";

        public event EventHandler NewRowAdded;
        public event EventHandler BeforeSave;
        public event EventHandler BeforeRefresh;

        private const string SelectedItemsPropertyName = "SelectedItems";
        private ILookup<string, string> _allowedPmMethods;
        private BillOperation[] _operations;
        private object[] _entityIds;
        private SysObject[] _attributes;

        public PmConfigViewModel()
        {
            PanelCaption = Properties.Resources.ConfiguratorPanelCaption;
            SelectedItems = new ObservableCollection<PmConfiguratorData>();
            DataForDelete = new Dictionary<string, PmConfiguratorData>();
            IsDeleted = new Dictionary<string, bool>();

            RefreshCommand = new DelegateCustomCommand(OnRefresh, CanRefresh);
            NewCommand = new DelegateCustomCommand(OnNewCommand, CanNewCommand);
            DeleteCommand = new DelegateCustomCommand(OnDelete, CanDelete);
            SaveCommand = new DelegateCustomCommand(OnSave, CanSave);

            Commands.AddRange(new[] { RefreshCommand, NewCommand, DeleteCommand, SaveCommand });

            CreateMainMenu();
            PropertyChanged += OnPropertyChanged;
        }

        #region . Properties .

        private ObservableCollection<PmConfiguratorData> _itemsSource;
        public ObservableCollection<PmConfiguratorData> ItemsSource
        {
            get { return _itemsSource; }
            set
            {
                if (_itemsSource == value)
                    return;
                _itemsSource = value;
                OnPropertyChanged("ItemsSource");
            }
        }

        private ObservableCollection<PmConfiguratorData> _selectedItems;
        public ObservableCollection<PmConfiguratorData> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                if (_selectedItems == value)
                    return;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;

                _selectedItems = value;

                if (_selectedItems != null)
                    _selectedItems.CollectionChanged += SelectedItemsCollectionChanged;

                OnPropertyChanged(SelectedItemsPropertyName);
            }
        }

        private string _totalRowItemAdditionalInfo;
        public string TotalRowItemAdditionalInfo
        {
            get { return _totalRowItemAdditionalInfo; }
            set
            {
                if (_totalRowItemAdditionalInfo == value)
                    return;
                _totalRowItemAdditionalInfo = value;
                OnPropertyChanged("TotalRowItemAdditionalInfo");
            }
        }

        private ObservableCollection<PM> _pms;
        public ObservableCollection<PM> Pms
        {
            get { return _pms ?? (_pms = new ObservableCollection<PM>()); }
            set
            {
                if (_pms == value)
                    return;
                _pms = value;
                OnPropertyChanged(PmsPropertyName);
            }
        }

        private ObservableCollection<PMMethod> _pmMethods;
        public ObservableCollection<PMMethod> PmMethods
        {
            get { return _pmMethods ?? (_pmMethods = new ObservableCollection<PMMethod>()); }
            set
            {
                if (_pmMethods == value)
                    return;
                _pmMethods = value;
                OnPropertyChanged(PmMethodsPropertyName);
            }
        }

        public Dictionary<string, PmConfiguratorData> DataForDelete { get; private set; }

        private IDictionary<string, bool> _detailPmMethodByProduct;

        /// <summary>
        /// Рзрешение использования дополнительных параметров методов.
        /// </summary>
        public IDictionary<string, bool> AllowedDetailsPmMethod
        {
            get { return _detailPmMethodByProduct ?? (_detailPmMethodByProduct = new Dictionary<string, bool>()); }
        }

        public IDictionary<string, bool> IsDeleted { get; private set; }
        #endregion . Properties .

        #region . Commands .
        public ICommand RefreshCommand { get; private set; }
        public ICommand NewCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand AppearanceStyleCommand { get; private set; }

        private bool CanRefresh()
        {
            var result = !WaitIndicatorVisible;
            if (result)
                OnBeforeSave();
            return result;
        }

        private void OnRefresh()
        {
            if (!CanRefresh())
                return;

            if (HasChanges(GetChanges()) && GetViewService().ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationSaveDataOnRefresh
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }

            OnBeforeRefresh();

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            GetData();
        }

        private void OnBeforeRefresh()
        {
            var handler = BeforeRefresh;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private bool CanNewCommand()
        {
            var result = !WaitIndicatorVisible && ItemsSource != null;
            if (result)
                OnBeforeSave();
            return result;
        }

        private void OnNewCommand()
        {
            if (!CanNewCommand())
                return;

            if (!ItemsSource.Any(p => p.IsNewRow))
            {
                var item = new PmConfiguratorData(this);
                item.AcceptChanges(true);
                ItemsSource.Add(item);
            }
            OnNewRowAdded();
        }

        private bool CanDelete()
        {
            var result = !WaitIndicatorVisible && SelectedItems != null && SelectedItems.Count > 0 && !SelectedItems.Any(p => !p.IsNew && p.IsDirty);
            if (result)
                OnBeforeSave();
            return result;
        }

        private void OnDelete()
        {
            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                if (!DeleteConfirmation()) 
                    return;

                var pm2Operations = new List<PM2Operation>();
                var pmConfigs = new List<PMConfig>();
                GetPmEntities(SelectedItems, item => item != null && !item.IsNew,  ref pm2Operations, ref pmConfigs);

                // удаляем запись        
                if (pm2Operations.Count != 0)
                {
                    using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
                    {
                        manager.DeletePmConfiguratorData(pm2Operations, pmConfigs);
                    }
                }

                for (; 0 < SelectedItems.Count; )
                    ItemsSource.Remove(SelectedItems[0]);
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemsCantDelete))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        private bool CanSave()
        {
            var result = !WaitIndicatorVisible && ItemsSource != null && ItemsSource.Count > 0 && HasChanges(GetChanges());
            if (result)
                OnBeforeSave();
            return result;
        }

        private void OnSave()
        {
            if (!CanSave())
                return;

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            Save(GetChanges());
        }

        private bool CanAppearanceStyle()
        {
            var result = !WaitIndicatorVisible && IsCustomizeEnabled;
            if (result)
                OnBeforeSave();
            return result;
        }

        private void OnAppearanceStyle()
        {
            if (!CanAppearanceStyle())
                return;
            IsCustomization = false;
            IsCustomization = true;
        }
        #endregion . Commands .

        #region . Methods .

        protected override void InitializeSettings()
        {
            //Используем глобальные настройки вида панели инструментов
            //MenuSuffix = GetType().GetFullNameWithoutVersion();

            base.InitializeSettings();

            IsMenuEnable = true;
            IsCustomizeBarEnabled = true;
        }

        protected override void SetupCustomizeMenu(BarItem bar, ListMenuItem listmenu)
        {
            base.SetupCustomizeMenu(bar, listmenu);

            if (AppearanceStyleCommand == null)
                AppearanceStyleCommand = new DelegateCustomCommand(OnAppearanceStyle, CanAppearanceStyle);

            // Добавляем функционал подсветки (в начало списка)
            var minPriority = listmenu.MenuItems.Min(i => i.Priority);
            listmenu.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.AppearanceStyle,
                Command = AppearanceStyleCommand,
                ImageSmall = ImageResources.DCLAppearanceStyle16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAppearanceStyle32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = minPriority - 100
            });

            // убираем функционал настройки самого layout-а
            var item = listmenu.MenuItems.FirstOrDefault(i => i.Caption == StringResources.CustomizeRegion);
            if (item != null)
                listmenu.MenuItems.Remove(item);
        }

        public void Initialize()
        {
           OnRefresh();
        }

        private string GetKey(string pmcode, string entitycode, string objectname)
        {
            return string.Format("{0}|{1}|{2}", pmcode, entitycode, objectname);
        }

        public BillOperation[] GetOperation()
        {
            if (_operations == null)
            {
                using (var billOperationManager = IoC.Instance.Resolve<IBaseManager<BillOperation>>())
                {
                    _operations = billOperationManager.GetFiltered("operationcode in (select distinct m2o.operationcode_r from wmspmmethod2operation m2o)", GetModeEnum.Partial).ToArray();
                }
            }
            return _operations;
        }

        private void OnNewRowAdded()
        {
            var handler = NewRowAdded;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnBeforeSave()
        {
            var handler = BeforeSave;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void CreateMainMenu()
        {
            InitializeCustomizationBar();

            var bar = Menu.GetOrCreateBarItem(StringResources.Commands, 1);

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.RefreshData,
                Command = RefreshCommand,
                ImageSmall = ImageResources.DCLFilterRefresh16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLFilterRefresh32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F5),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1
            });

            bar.MenuItems.Add(new SeparatorMenuItem {Priority = 2});

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.New,
                Command = NewCommand,
                ImageSmall = ImageResources.DCLAddNew16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLAddNew32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 3
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Delete,
                Command = DeleteCommand,
                ImageSmall = ImageResources.DCLDelete16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLDelete32.GetBitmapImage(),
                HotKey = new KeyGesture(Key.F9),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 4
            });

            bar.MenuItems.Add(new SeparatorMenuItem {Priority = 5});

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Save,
                Command = SaveCommand,
                HotKey = new KeyGesture(Key.F6),
                ImageSmall = ImageResources.DCLSave16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLSave32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 6
            });

            //bar.MenuItems.Add(new CommandMenuItem
            //{
            //    Caption = StringResources.SaveAndClose,
            //    Command = SaveAndCloseCommand,
            //    HotKey = new KeyGesture(Key.F7),
            //    ImageSmall = ImageResources.DCLSaveAndClose16.GetBitmapImage(),
            //    ImageLarge = ImageResources.DCLSaveAndClose32.GetBitmapImage(),
            //    GlyphAlignment = GlyphAlignmentType.Top,
            //    DisplayMode = DisplayModeType.Default,
            //    Priority = 5
            //});
        }

        private async void GetData()
        {
            ItemsSource = null;

            try
            {
                WaitStart();
                IsDeleted.Clear();
                DataForDelete.Clear();
                var data = await GetDataAsync();
                if (data == null)
                    return;

                var itemsSource = new ObservableCollection<PmConfiguratorData>(data);
                AcceptChanges(itemsSource);
                ItemsSource = itemsSource;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantRefresh))
                    throw;
            }
            finally
            {
                WaitStop();
            }
        }

        private void AcceptChanges(IEnumerable<PmConfiguratorData> itemsSource)
        {
            if (itemsSource != null)
            {
                itemsSource.ForEach(p =>
                {
                    p.AcceptChanges();
                    foreach (var v in p.PmMethodCodes.Values)
                    {
                        v.AcceptChanges();
                    }
                });
            }
        }

        private async Task<IEnumerable<PmConfiguratorData>> GetDataAsync()
        {
            var now = DateTime.Now;
            IEnumerable<BillOperation> operations = null;
            IEnumerable<decimal> entityids = null;
            IEnumerable<SysObject> attributes = null;
            IEnumerable<PM> pms = null;
            IEnumerable<PMMethod> pmMethods = null;
            IEnumerable<PMMethod2Operation> detailsPmMethod = null;
            DataTable pmdatatable = null;
            DataTable pmMethod2OperationsAlloweddatatable = null;
            double lastQueryExecutionTime = 0;

            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    //System.Threading.Thread.Sleep(10000);
                    using (var mng = IoC.Instance.Resolve<IBPProcessManager>())
                    {
                        mng.GetPmConfiguratorData(ref operations, ref entityids, ref attributes,
                            ref pms, ref pmMethods,
                            ref detailsPmMethod, ref pmdatatable, ref pmMethod2OperationsAlloweddatatable);

                        lastQueryExecutionTime = ((IBaseManager) mng).LastQueryExecutionTime;
                    }

                    //Получаем операции
                    _operations = null;
                    if (operations != null)
                        _operations = operations.ToArray();

                    //Получаем PM
                    Pms = null;
                    if (pms != null)
                        Pms = new ObservableCollection<PM>(pms.ToArray());

                    //Получаем сущности
                    _entityIds = null;
                    if (entityids != null)
                        _entityIds = entityids.Cast<object>().ToArray();

                    //Получаем атрибуты
                    _attributes = null;
                    if (attributes != null)
                        _attributes = attributes.ToArray();

                    //Методы
                    PmMethods = null;
                    if (pmMethods != null)
                    {
                        var pmMethodsInternal = pmMethods.ToList();
                        var method = new PMMethod
                        {
                            PMMETHODNAME = Properties.Resources.PmMethodIsUnavailable
                        };
                        method.SetKey(method.PMMETHODNAME);
                        pmMethodsInternal.Insert(0, method);
                        PmMethods = new BaseObservableCollection<PMMethod>(pmMethodsInternal);
                    }

                    //Получаем детализацию настройки методов
                    AllowedDetailsPmMethod.Clear();
                    if (detailsPmMethod != null)
                    {
                        foreach (PMMethod2Operation p2O in detailsPmMethod)
                        {
                            var key = ConfiguratorHelper.CreateAllowedDetailsPmMethodKey(
                                operationCode: p2O.PMMETHOD2OPERATIONOPERATIONCODE,
                                objectEntityCode: p2O.VOBJECTENTITYCODE,
                                objectName: p2O.VOBJECTNAME,
                                methodCode: p2O.PMMETHOD2OPERATIONPMMETHODCODE,
                                property: PMConfig.PMCONFIGBYPRODUCTPropertyName);
                            AllowedDetailsPmMethod[key] = p2O.PMMETHOD2OPERATIONBYPRODUCT == true;

                            key = ConfiguratorHelper.CreateAllowedDetailsPmMethodKey(
                                operationCode: p2O.PMMETHOD2OPERATIONOPERATIONCODE,
                                objectEntityCode: p2O.VOBJECTENTITYCODE,
                                objectName: p2O.VOBJECTNAME,
                                methodCode: p2O.PMMETHOD2OPERATIONPMMETHODCODE,
                                property: PMConfig.PMCONFIGINPUTMASKPropertyName);
                            AllowedDetailsPmMethod[key] = p2O.PMMETHOD2OPERATIONINPUTMASK == true;

                            key = ConfiguratorHelper.CreateAllowedDetailsPmMethodKey(
                                operationCode: p2O.PMMETHOD2OPERATIONOPERATIONCODE,
                                objectEntityCode: p2O.VOBJECTENTITYCODE,
                                objectName: p2O.VOBJECTNAME,
                                methodCode: p2O.PMMETHOD2OPERATIONPMMETHODCODE,
                                property: PMConfig.PMCONFIGINPUTMASSPropertyName);
                            AllowedDetailsPmMethod[key] = p2O.PMMETHOD2OPERATIONINPUTMASS == true;
                        }
                    }

                    _allowedPmMethods = null;
                    if (pmMethod2OperationsAlloweddatatable != null)
                    {
                        _allowedPmMethods =
                            pmMethod2OperationsAlloweddatatable.AsEnumerable()
                                .ToLookup(
                                    key =>
                                        ConfiguratorHelper.CreatePmAllowedMethodsKey(key.Field<string>("OperationCode"),
                                            key.Field<string>("ObjectEntityCode"),
                                            key.Field<string>("ObjectName")),
                                    g => g.Field<string>("PmMethodCode"));
                    }

                    if (pmdatatable == null)
                        return null;

                    //Поготовка ItemsSource
                    var data = pmdatatable.AsEnumerable()
                        .Select(p => new
                        {
                            PMCODE = p.Field<string>("PMCODE"),
                            PM2OPERATIONCODE = p.Field<string>("PM2OPERATIONCODE"),
                            OPERATIONCODE = p.Field<string>("OPERATIONCODE"),
                            OBJECTENTITYCODE = p.Field<string>("OBJECTENTITYCODE"),
                            OBJECTNAME = p.Field<string>("OBJECTNAME"),
                            PMMETHODCODE = p.Field<string>("PMMETHODCODE"),
                            BYPRODUCT = (p.Field<short?>("PMCONFIGBYPRODUCT") >= 1),
                            INPUTMASK = (p.Field<string>("PMCONFIGINPUTMASK")),
                            INPUTMASS = (p.Field<short?>("PMCONFIGINPUTMASS") >= 1)
                        })
                        //.Where(p => !string.IsNullOrEmpty(p.OBJECTENTITYCODE) && !string.IsNullOrEmpty(p.OBJECTNAME))
                        .GroupBy(key => GetKey(key.PMCODE, key.OBJECTENTITYCODE, key.OBJECTNAME))
                        .Select(g =>
                        {
                            var lkmethods = g
                                .Where(p => !string.IsNullOrEmpty(p.OPERATIONCODE))
                                .ToLookup(lk => lk.OPERATIONCODE, lv => lv.PMMETHODCODE);
                            var methodsdict = new Dictionary<string, EditableBusinessObjectCollection<object>>();
                            foreach (var l in lkmethods)
                            {
                                methodsdict[l.Key] =
                                    new EditableBusinessObjectCollection<object>(l.Select(lv => (object) lv));
                                methodsdict[l.Key].AcceptChanges();
                            }

                            var pmConfiguratorData = new PmConfiguratorData(this);

                            try
                            {
                                pmConfiguratorData.BeginDataUpdate();
                                pmConfiguratorData.PmCode = g.First().PMCODE;
                                pmConfiguratorData.OjectEntityCode = g.First().OBJECTENTITYCODE;
                                pmConfiguratorData.ObjectName = g.First().OBJECTNAME;
                                pmConfiguratorData.PmMethodCodes = methodsdict;

                                foreach (var bp in g.Where(p => p.BYPRODUCT))
                                {
                                    pmConfiguratorData.PmMethodByProduct[
                                        pmConfiguratorData.GetPmMethodDetailsKey(operationCode: bp.OPERATIONCODE,
                                            methodCode: bp.PMMETHODCODE)] = true;
                                }

                                foreach (var bp in g.Where(p => !string.IsNullOrEmpty(p.INPUTMASK)))
                                {
                                    pmConfiguratorData.PmMethodByInputMask[
                                        pmConfiguratorData.GetPmMethodDetailsKey(operationCode: bp.OPERATIONCODE,
                                            methodCode: bp.PMMETHODCODE)] = bp.INPUTMASK;
                                }

                                foreach (var bp in g.Where(p => p.INPUTMASS))
                                {
                                    pmConfiguratorData.PmMethodByInputMass[
                                        pmConfiguratorData.GetPmMethodDetailsKey(operationCode: bp.OPERATIONCODE,
                                            methodCode: bp.PMMETHODCODE)] = true;
                                }
                            }
                            finally
                            {
                                pmConfiguratorData.EndDataUpdate();
                            }
                            return pmConfiguratorData;
                        }).ToList();

                    return data;
                }
                finally
                {
                    TotalRowItemAdditionalInfo =
                           string.Format(StringResources.ListViewModelBaseTotalRowItemAdditionalInfo,
                               (DateTime.Now - now).TotalSeconds, lastQueryExecutionTime);
                }
            });
        }

        public object[] GetEntityIds()
        {
            return _entityIds ?? new object[0];
        }

        public object[] GetAttributes(string objectEntityCode)
        {
            return _attributes == null ? null : _attributes.Where(p => p.ObjectEntityCode == objectEntityCode).Select(p => p.GetKey()).ToArray();
        }

        public bool ValidatePmMethods(string operationCode, PmConfiguratorData data)
        {
            if (data == null)
                return false;

            var result = GetAllowedPmMethods(operationCode: operationCode, objectEntityCode: data.OjectEntityCode, objectName: data.ObjectName);
            return result.Any();
        }

        public string[] GetAllowedPmMethods(string operationCode, string objectEntityCode, string objectName)
        {
            var emptyresult = new string[0];
            if (_allowedPmMethods == null || !_allowedPmMethods.Any())
                return emptyresult;

            var key = ConfiguratorHelper.CreatePmAllowedMethodsKey(operationCode, objectEntityCode, objectName);
            return _allowedPmMethods.Contains(key) ? _allowedPmMethods[key].ToArray() : emptyresult;
        }

        private PmConfiguratorData[] GetChanges()
        {
            if (ItemsSource == null)
                return null;

            return ItemsSource.Where(p => p != null && p.HasChanges()).ToArray();
        }

        private bool HasChanges(ICollection<PmConfiguratorData> changes)
        {
            return changes != null && changes.Count > 0;
        }

        private void GetPmEntities(IEnumerable<PmConfiguratorData> data, Func<PmConfiguratorData, bool> whereFunc, ref List<PM2Operation> pm2Operations, ref List<PMConfig> pmConfigs)
        {
            if (data == null)
                return;

            if (pm2Operations == null)
                pm2Operations = new List<PM2Operation>();
            if (pmConfigs == null)
                pmConfigs = new List<PMConfig>();

            var whereFuncinternal = whereFunc ?? (item => item != null);

            foreach (var p in data.Where(whereFuncinternal))
            {
                foreach (var op in p.PmMethodCodes)
                {
                    //if (!ValidatePmMethods(op.Key, p))
                    //    continue;

                    var pm2Operation = new PM2Operation
                    {
                        OperationCode_r = op.Key
                    };

                    var pm2OperationCode = string.Format("-{0}:{1}", op.Key, p.PmCode);
                    pm2Operation.SetProperty(pm2Operation.GetPrimaryKeyPropertyName(), pm2OperationCode);
                    pm2Operation.PM2OperationPMCode = p.PmCode;
                    pm2Operations.Add(pm2Operation);

                    var methods = op.Value;
                    if (methods == null)
                        continue;

                    //Если убрали метод
                    //if (methods.Count == 0)
                    //    methods.Add(string.Empty);

                    foreach (var m in methods.Cast<string>())
                    {
                        var pmbyproductkey = p.GetPmMethodDetailsKey(operationCode: op.Key, methodCode: m);
                        var pmConfig = new PMConfig
                        {
                            PM2OperationCode_r = pm2OperationCode,
                            ObjectEntitycode_R = p.OjectEntityCode,
                            ObjectName_r = p.ObjectName,
                            MethodCode_r = m,
                            PMCONFIGBYPRODUCT = p.PmMethodByProduct.ContainsKey(pmbyproductkey)
                                ? p.PmMethodByProduct[pmbyproductkey]
                                : (bool?)null,
                            PMCONFIGINPUTMASK = p.PmMethodByInputMask.ContainsKey(pmbyproductkey)
                                ? p.PmMethodByInputMask[pmbyproductkey] 
                                : null,
                            PMCONFIGINPUTMASS = p.PmMethodByInputMass.ContainsKey(pmbyproductkey)
                                ? p.PmMethodByInputMass[pmbyproductkey]
                                : (bool?)null,
                        };

                        pmConfigs.Add(pmConfig);
                    }
                }
            }
        }

        private bool Save(ICollection<PmConfiguratorData> changes)
        {
            if (!HasChanges(changes))
                return true;

            try
            {
                WaitStart();

                var pm2Operations = new List<PM2Operation>();
                var pmConfigs = new List<PMConfig>();
                GetPmEntities(changes, null, ref pm2Operations, ref pmConfigs);

                var deletePm2Operations = new List<PM2Operation>();
                var deletePmConfigs = new List<PMConfig>();
                var dataForDelete = DataForDelete.Select(p => p.Value).ToArray();
                GetPmEntities(dataForDelete, (item => item != null && !string.IsNullOrEmpty(item.PmCode)), ref deletePm2Operations, ref deletePmConfigs);

                List<string> errors;
                using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    errors = manager.SavePmConfiguratorData(pm2Operations, pmConfigs, deletePm2Operations, deletePmConfigs);
                }

                if (errors != null && errors.Count > 0)
                {
                    var message = string.Format(Properties.Resources.ValidationError, Environment.NewLine,
                        string.Join(Environment.NewLine, errors.Distinct()));

                    var vs = GetViewService();
                    vs.ShowDialog(StringResources.Error
                        , message
                        , MessageBoxButton.OK
                        , MessageBoxImage.Error
                        , MessageBoxResult.Yes);

                    return false;
                }

                AcceptChanges(ItemsSource);
                IsDeleted.Clear();
                DataForDelete.Clear();

                return true;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantSave))
                    throw;
                return false;
            }
            finally
            {
                WaitStop();
             }
        }

        protected override bool CanCloseInternal()
        {
            var result = base.CanCloseInternal();
            if (!result)
                return false;

            if (!CanSave())
                return true;

            var changes = GetChanges();
            if (!HasChanges(changes))
                return true;

            try
            {
                WaitStart();

                var vs = GetViewService();
                var dr = vs.ShowDialog(StringResources.Confirmation
                    , StringResources.ConfirmationUnsavedData
                    , MessageBoxButton.YesNoCancel
                    , MessageBoxImage.Question
                    , MessageBoxResult.Yes);

                switch (dr)
                {
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.Yes:
                        return Save(changes);
                    case MessageBoxResult.No:
                        return true;
                    default:
                        throw new DeveloperException(DeveloperExceptionResources.UnknownDialogResult);
                }
            }
            finally
            {
                WaitStop();
            }
        }

        private bool DeleteConfirmation()
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Confirmation
                , string.Format(StringResources.ConfirmationDeleteRecords, SelectedItems.Count)
                , MessageBoxButton.YesNo //MessageBoxButton.YesNoCancel
                , MessageBoxImage.Question
                , MessageBoxResult.Yes);

            return dr == MessageBoxResult.Yes;
        }

        private void SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Reset)
                RiseCommandsCanExecuteChanged();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            RiseCommandsCanExecuteChanged();
        }

        public void RiseCommandsCanExecute()
        {
            RiseCommandsCanExecuteChanged();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _itemsSource = null;
            PropertyChanged -= OnPropertyChanged;
            if (_selectedItems != null)
                _selectedItems.CollectionChanged -= SelectedItemsCollectionChanged;

            _operations = null;
            _allowedPmMethods = null;
            _entityIds = null;
            Pms = null;
            _attributes = null;
            PmMethods = null;
        }

        #endregion . Methods .
    }

    #region . class PmConfiguratorData .
    public class PmConfiguratorData : EditableBusinessObject, IKeyHandler, ICloneable
    {
        public const string PmCodePropertyName = "PmCode";
        public const string ObjectEntityCodePropertyName = "OjectEntityCode";
        public const string ObjectNamePropertyName = "ObjectName";
        public const string PmMethodCodesPropertyName = "PmMethodCodes";
        public const string IsNewRowPropertyName = "IsNewRow";

        private bool _beginDataUpdate;

        public PmConfiguratorData(PmConfigViewModel owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            Owner = owner;
            PmMethodCodes = new Dictionary<string, EditableBusinessObjectCollection<object>>();
            PmMethodByProduct = new Dictionary<string, bool>();
            PmMethodByInputMask = new Dictionary<string, string>();
            PmMethodByInputMass = new Dictionary<string, bool>();
        }

        public PmConfigViewModel Owner { get; private set; }

        public string Id
        {
            get { return string.Format("'{0}'_'{1}'_'{2}'", PmCode, OjectEntityCode, ObjectName); }
        }

        private string _pmCode;
        public string PmCode
        {
            get { return _pmCode; }
            set
            {
                if (_pmCode == value)
                    return;
                _pmCode = value;
                OnPropertyChanged(PmCodePropertyName);

                if (_beginDataUpdate)
                    return;
                OjectEntityCode = null;
                ObjectName = null;
                ClearPmMethodCodes();
            }
        }

        private string _objectEntityCode;
        public string OjectEntityCode
        {
            get { return _objectEntityCode; }
            set
            {
                if (_objectEntityCode == value)
                    return;
                _objectEntityCode = value;
                OnPropertyChanged(ObjectEntityCodePropertyName);

                if (_beginDataUpdate)
                    return;
                ObjectName = null;
                ClearPmMethodCodes();
            }
        }

        private string _objectName;
        public string ObjectName
        {
            get { return _objectName; }
            set
            {
                if (_objectName == value)
                    return;
                _objectName = value;
                OnPropertyChanged(ObjectNamePropertyName);

                if (_beginDataUpdate)
                    return;
                ClearPmMethodCodes();
            }
        }

        private Dictionary<string, EditableBusinessObjectCollection<object>> _pmMethodCodes;
        public Dictionary<string, EditableBusinessObjectCollection<object>> PmMethodCodes
        {
            get { return _pmMethodCodes; }
            set
            {
                if (_pmMethodCodes == value)
                    return;
                _pmMethodCodes = value;
                OnPropertyChanged(PmMethodCodesPropertyName);
            }
        }

        /// <summary>
        /// Словарь свойств сущности PMConfig: ключ - operationCode + methodCode (см. GetPmMethodByProductKey), значение - PMCONFIGBYPRODUCT.
        /// </summary>
        public Dictionary<string, bool> PmMethodByProduct { get; private set; }

        /// <summary>
        /// Словарь свойств сущности PMConfig: ключ - operationCode + methodCode (см. GetPmMethodByProductKey), значение - PMCONFIGINPUTMASK.
        /// </summary>
        public Dictionary<string, string> PmMethodByInputMask { get; private set; }

        /// <summary>
        /// Словарь свойств сущности PMConfig: ключ - operationCode + methodCode (см. GetPmMethodByProductKey), значение - PMCONFIGINPUTMASS.
        /// </summary>
        public Dictionary<string, bool> PmMethodByInputMass { get; private set; }

        public PmMethodViewModel MethodViewModel { get; set; }

        public bool IsNewRow
        {
            get
            {
                return IsNew && !IsDirty;
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName != IsDirtyPropertyName)
                IsDirty = true;
        }

        public void BeginDataUpdate()
        {
            _beginDataUpdate = true;
        }

        public void EndDataUpdate()
        {
            _beginDataUpdate = false;
        }

        private void ClearPmMethodCodes()
        {
            PmMethodCodes.Clear();
            OnPropertyChanged(PmMethodCodesPropertyName);
        }

        public string GetPmMethodDetailsKey(string operationCode, string methodCode)
        {
            return string.Format("'{0}'_'{1}'", operationCode, methodCode);
        }

        public void PmMethodCodesPropertyChanged()
        {
            OnPropertyChanged(PmMethodCodesPropertyName);
        }

        public bool HasChanges()
        {
            return !string.IsNullOrEmpty(PmCode) &&
                   (IsNew || IsDirty ||
                    (PmMethodCodes != null &&
                     PmMethodCodes.Any(m => m.Value != null && (m.Value.IsDirty || m.Value.IsNew))));
        }

        #region . ICloneable .

        public PmConfiguratorData Clone()
        {
            try
            {
                BeginDataUpdate();
                var result = new PmConfiguratorData(Owner)
                {
                    PmCode = PmCode,
                    OjectEntityCode = OjectEntityCode,
                    ObjectName = ObjectName,
                    PmMethodCodes = new Dictionary<string, EditableBusinessObjectCollection<object>>((PmMethodCodes))
                };

                return result;
            }
            finally
            {
                EndDataUpdate();
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion . ICloneable .

        public override object GetProperty(string name)
        {
            return null;
        }

        #region . IKeyHandler .
        string IKeyHandler.GetPrimaryKeyPropertyName()
        {
            throw new NotImplementedException();
        }

        object IKeyHandler.GetKey()
        {
            return Id;
        }

        TKey IKeyHandler.GetKey<TKey>()
        {
            return (TKey)((IKeyHandler)this).GetKey();
        }

        void IKeyHandler.SetKey(object o)
        {
            throw new NotImplementedException();
        }

        bool IKeyHandler.HasPrimaryKey()
        {
            return true;
        }
        #endregion . IKeyHandler .
    }
    #endregion . class PmConfiguratorData .
}
