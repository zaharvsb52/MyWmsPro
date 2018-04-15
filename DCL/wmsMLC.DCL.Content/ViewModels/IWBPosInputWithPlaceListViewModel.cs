using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.XtraEditors.DXErrorProvider;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Managers.Validation;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(IWBPosInputWithPlaceListView))]
    public class IWBPosInputWithPlaceListViewModel : CustomObjectListViewModelBase<IWBPosInput>, IDialogResultHandler
    {
        #region .  Fields  .
        private Place _selectedPlace;
        private string _placeFilter;
        private string _operationCode;
        private List<WorkingInfo> _workings;
        private CommandMenuItem _endCommandMenuItem;
        private CommandMenuItem _beginCommandMenuItem;
        private decimal? _posID;
        private string _timeText;
        private readonly System.Timers.Timer _timer;
        private DateTime _startTime;
        private bool _isEnabled;
        #endregion .  Fields  .

        #region . Properties  .

        public bool PrintTE { get; set; }

        public string PrintCaption { get; set; }

        public string PlaceCaption { get; set; }

        public string PositionCaption { get; set; }

        public decimal? MandantId { get; set; }

        public Place CurrentPlace { get; set; }

        public bool IsMigration { get; set; }

        public ObservableCollection<DataField> SubFields { get; set; }

        public Place SelectedPlace
        {
            get
            {
                return _selectedPlace;
            }
            set
            {
                _selectedPlace = value;
                OnPropertyChanged("SelectedPlace");
            }
        }

        public string PlaceFilter
        {
            get
            {
                return _placeFilter;
            }
            set
            {
                _placeFilter = value;
                OnPropertyChanged("PlaceFilter");
            }
        }

        public string OperationCode
        {
            get
            {
                return _operationCode;
            }
            set
            {
                _operationCode = value;
                OnPropertyChanged("OperationCode");
            }
        }

        public List<WorkingInfo> Workings
        {
            get { return _workings; }
            set
            {
                _workings = value;
                OnPropertyChanged("Workings");
            }
        }

        public string TimeText
        {
            get { return _timeText; }
            set
            {
                _timeText = value;
                OnPropertyChanged("TimeText");
            }
        }
        
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (IsEnabled == value)
                    return;
                _isEnabled = value;

                _beginCommandMenuItem.IsVisible = !IsEnabled;
                _endCommandMenuItem.IsVisible = IsEnabled;

                OnPropertyChanged("IsEnabled");
            }
        }

        public bool? DialogResult { get; set; }
 
        /// <summary>
        /// Код workflow для расшифровки batch-кодов.
        /// </summary>
        public string BatchcodeWorkflowCode { get; set; }

        /// <summary>
        /// Код workflow для изменения ОВХ SKU.
        /// </summary>
        public string SkuChangeMpWorkflowCode { get; set; }

        public ICommand BatchCommand { get; set; }

        public ICommand BeginWorkCommand { get; private set; }

        public ICommand EndWorkCommand { get; private set; }

        public ICommand ChangeOvxSkuCommand { get; private set; }
        #endregion . Properties  .

        #region .  Methods  .

        public override void InitializeMenus()
        {
            MenuSuffix = GetType().GetFullNameWithoutVersion();
            base.InitializeMenus();
        }

        public IWBPosInputWithPlaceListViewModel()
        {
            DialogResult = null;
            PrintCaption = "Печатать этикетки для ТЕ";
            PlaceCaption = "Место приемки";
            PositionCaption = "Выберите принимаемые позиции";
            // фильтр по умолчанию
            // задается из вне через активити
            PlaceFilter = "STATUSCODE_R = 'PLC_FREE'";

            BatchCommand = new DelegateCustomCommand(OnBatchCommand, CanBatchCommand);
            BeginWorkCommand = new DelegateCustomCommand<bool?>(BeginWork, CanBeginWork);
            EndWorkCommand = new DelegateCustomCommand(EndWork, CanEndWork);
            ChangeOvxSkuCommand = new DelegateCustomCommand(OnChangeOvxSkuCommand, CanChangeOvxSkuCommand);
            Commands.AddRange(new[] {BatchCommand, ChangeOvxSkuCommand});

            SubFields = GetSublistViewFields();

            _timer = new System.Timers.Timer(1000) { AutoReset = true };
            _timer.Elapsed += delegate
            {
                TimeText = (DateTime.Now - _startTime).ToString(@"mm\:ss");
            };
        }

        private bool CanBatchCommand()
        {
            return IsEnabled && !string.IsNullOrEmpty(BatchcodeWorkflowCode) && HasSelectedItems();
        }

        private void OnBatchCommand()
        {
            if (!CanBatchCommand())
                return;

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            Action<CompleteContext> completedHandler = ctx => WaitStop();

            try
            {
                WaitStart();
                var bpContext = new BpContext
                {
                    Items = SelectedItems.ToArray(),
                    DoNotRefresh = true
                };
                bpContext.Set("ShowErrorBatchcodeIsNull", true);
                var executionContext = new ExecutionContext(BatchcodeWorkflowCode,
                    new Dictionary<string, object>
                    {
                        { BpContext.BpContextArgumentName, bpContext }
                    });
                var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
                engine.Run(context: executionContext, completedHandler: completedHandler);
            }
            catch (Exception)
            {
                WaitStop();
                throw;
            }
        }

        private bool CanChangeOvxSkuCommand()
        {
            return IsEnabled && !string.IsNullOrEmpty(SkuChangeMpWorkflowCode) && SelectedItems != null && SelectedItems.Count == 1;
        }

        private void OnChangeOvxSkuCommand()
        {
            if (!CanChangeOvxSkuCommand())
                return;

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            Action<CompleteContext> completedHandler = ctx =>
            {
                try
                {
                    if (ctx.Parameters != null && ctx.Parameters.ContainsKey(BpContext.BpContextArgumentName))
                    {
                        var isResultOk = false;

                        Action<IWBPosInput, string, object> updatePropertyHandler = (iwbPosInputEntity, iwbPosInputPopertyName, skuPopertyValue) =>
                        {
                            try
                            {
                                iwbPosInputEntity.SuspendNotifications();
                                iwbPosInputEntity.SetProperty(iwbPosInputPopertyName, skuPopertyValue);
                            }
                            finally
                            {
                                iwbPosInputEntity.ResumeNotifications();
                            }
                        };

                        var bpcontext = ctx.Parameters[BpContext.BpContextArgumentName] as BpContext;
                        if (bpcontext != null)
                            isResultOk = bpcontext.Get<bool>("IsResultOk");
                        if (isResultOk && bpcontext.Items != null)
                        {
                            var items = bpcontext.Items.OfType<SKU>().ToArray();
                            if (items.Length > 0)
                            {
                                var sku = items[0];
                                foreach (var item in Source.Where(p => p.SKUID == sku.SKUID))
                                {
                                    updatePropertyHandler(item, IWBPosInput.VSKUHEIGHTPropertyName,
                                        sku.GetProperty(SKU.SKUHEIGHTPropertyName));
                                    updatePropertyHandler(item, IWBPosInput.VSKULENGTHPropertyName,
                                        sku.GetProperty(SKU.SKULENGTHPropertyName));
                                    updatePropertyHandler(item, IWBPosInput.VSKUWIDTHPropertyName,
                                        sku.GetProperty(SKU.SKUWIDTHPropertyName));
                                }
                            }
                        }
                    }
                }
                finally
                {
                    WaitStop();
                }
            };

            try
            {
                WaitStart();

                var skuid = SelectedItems[0].SKUID;
                var bpContext = new BpContext
                {
                    Items = new object[] {new SKU {SKUID = skuid ?? 0}},
                    DoNotRefresh = false
                };
                bpContext.Set("NeedRefreshSku", true);

                var executionContext = new ExecutionContext(SkuChangeMpWorkflowCode,
                    new Dictionary<string, object>
                    {
                        { BpContext.BpContextArgumentName, bpContext }
                    });
                var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
                engine.Run(context: executionContext, completedHandler: completedHandler);
            }
            catch (Exception)
            {
                WaitStop();
                throw;
            }
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();

            if (!string.IsNullOrEmpty(BatchcodeWorkflowCode))
            {
                var barBatchCode = Menu.GetOrCreateBarItem(StringResources.BatchCodeBarCaption, 2);
                barBatchCode.MenuItems.Add(new CommandMenuItem
                {
                    Caption = StringResources.BatchCodeMenuCaption,
                    Command = BatchCommand,
                    ImageSmall = ImageResources.DCLBatchProcessParse16.GetBitmapImage(),
                    ImageLarge = ImageResources.DCLBatchProcessParse32.GetBitmapImage(),
                    //HotKey = new KeyGesture(Key.F7),
                    GlyphAlignment = GlyphAlignmentType.Top,
                    DisplayMode = DisplayModeType.Default,
                    Priority = 0,
                });
            }

            var barCommand = Menu.GetOrCreateBarItem(StringResources.WorkCommands, 1);
            _beginCommandMenuItem = new CommandMenuItem
            {
                Caption = StringResources.BeginWork,
                Command = BeginWorkCommand,
                CommandParameter = true,
                ImageSmall = ImageResources.DCLWorkOpen16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkOpen32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = !IsEnabled,
                Priority = 1
            };
            barCommand.MenuItems.Add(_beginCommandMenuItem);

            _endCommandMenuItem = new CommandMenuItem
            {
                Caption = StringResources.EndWork,
                Command = EndWorkCommand,
                ImageSmall = ImageResources.DCLWorkClose16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkClose32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                IsVisible = IsEnabled,
                Priority = 1
            };
            barCommand.MenuItems.Add(_endCommandMenuItem);

            barCommand.MenuItems.Add(new SeparatorMenuItem { Priority = 2 });

            var barChangeOvxSku = Menu.GetOrCreateBarItem(StringResources.SkuCaption, 3);
            barChangeOvxSku.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.ChangeOvxSkuCaption,
                Command = ChangeOvxSkuCommand,
                ImageSmall = ImageResources.DCLChangeOvxSku16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLChangeOvxSku32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 2
            });
        }
        
        protected override void OnSourceChanged()
        {
            //HACK: т.к. у нас совсем кастомный объект и заполнен произвольно, скажем что он новый
            //foreach (var item in Source)
            //{
            //    var editable = item as IEditable;
            //    if (editable != null)
            //        editable.AcceptChanges();
            //}
            var validatable = Source as IValidatable;
            if (validatable != null)
                validatable.Validate();

            var eo = Source as IEditable;
            if (eo != null)
                OnIsDirtyChanged(eo);
        }

        protected override bool CanCloseInternal()
        {
            if ((DialogResult == null || DialogResult == false) &&
               ViewService.ShowDialog(StringResources.Confirmation,
                   string.Format(StringResources.CloseFromConfirmationFormat, Environment.NewLine),
                   MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.No)
                return false;

            // корректно выходим - ничего делать не нужно
            if (DialogResult != true)
                return true;

            var errorMessage = new StringBuilder();
            var properties = TypeDescriptor.GetProperties(typeof(IWBPosInput));
            using (var pmConfigMgr = IoC.Instance.Resolve<IPMConfigManager>())
            {
                foreach (var item in SelectedItems)
                {
                    // будем выставлять дефолтное значение, если объект новый
                    var eo = item as IIsNew;
                    if (eo != null && eo.IsNew)
                        DefaultValueSetter.Instance.SetDefaultValues(item);

                    if (IsMigration)
                        continue;

                    var mustPropertyList = pmConfigMgr.GetPMConfigByParamListByArtCode(item.ArtCode, OperationCode, "MUST_SET");
                    foreach (var mustProperty in mustPropertyList)
                    {
                        var p = properties.Find(mustProperty.ObjectName_r, true);
                        if (p != null)
                        {
                            if (item.GetProperty(mustProperty.ObjectName_r) == null)
                            {
                                errorMessage.AppendFormat("В строке номер {0} не заполнено обязательное поле '{1}' :{2}.{3}",
                                    Source.IndexOf(item) + 1, p.DisplayName, mustProperty.ObjectName_r, Environment.NewLine);
                            }
                        }
                        else
                        {
                            errorMessage.AppendFormat("Ошибка в настройках MUST_SET менеджера товара.{1}Задан неизвестный параметр '{0}'.", mustProperty.ObjectName_r, Environment.NewLine);
                        }
                    }
                }
            }

            if (errorMessage.Length > 0)
            {
                ViewService.ShowDialog("Ошибка", string.Format("Ошибки:{0}{1}", Environment.NewLine, errorMessage), MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return false;
            }
            return true;
        }

        protected override async Task<IViewModel> WrappModelIntoVM(IWBPosInput model)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (ObjectViewModel != null)
                {
                    var disposable = ObjectViewModel as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                    ObjectViewModel = null;
                }
                ObjectViewModel = (IObjectViewModel<IWBPosInput>)IoC.Instance.Resolve(typeof(IWBPosInputViewModel));
                ObjectViewModel.SetSource(model);
                
                var md = (IWBPosInputViewModel) ObjectViewModel;
                md.MandantId = MandantId;
                md.CanUseBatch = !string.IsNullOrEmpty(BatchcodeWorkflowCode);
                md.CanChangeOvxSkuHanler = CanChangeOvxSkuCommand;
                md.OnChangeOvxSkuHanler = OnChangeOvxSkuCommand;

                return ObjectViewModel;
            });
        }

        protected override async Task<IViewModel> WrappModelIntoVM(IEnumerable<IWBPosInput> model)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (ObjectViewModel != null)
                {
                    var disposable = ObjectViewModel as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                    ObjectViewModel = null;
                }
                ObjectViewModel = (IObjectViewModel<IWBPosInput>)IoC.Instance.Resolve(typeof(IWBPosInputViewModel));
                var pem = ObjectViewModel as IPropertyEditHandler;
                if (pem == null)
                    throw new DeveloperException("Модель не реализует IPropertyEditHandler");
                pem.SetSource(model);

                var md = (IWBPosInputViewModel)ObjectViewModel;
                md.MandantId = MandantId;
                md.CanUseBatch = !string.IsNullOrEmpty(BatchcodeWorkflowCode);
                //Если SelectedItems.Count > 1 не даем редактировать SKU

                return ObjectViewModel;
            });
        }

        protected override IWBPosInput CreateNewItem()
        {
            return new IwbPosInputErrorInfo(base.CreateNewItem());
        }
        
        public override async void RefreshData()
        {
            try
            {
                WaitStart();
                Workings = RefreshWorking();
            }
            finally
            {
                WaitStop();
            }
        }

        private List<WorkingInfo> RefreshWorking()
        {
            if (!_posID.HasValue)
            {
                var posList = Source as IEnumerable<IWBPosInput>;
                if (posList == null)
                    return new List<WorkingInfo>();

                var pos = posList.FirstOrDefault(i => i.IWBPosId > 0);
                if (pos == null)
                    return new List<WorkingInfo>();

                _posID = pos.GetKey<decimal>();
            }

            List<Working> workingList;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                workingList = mgr.GetFiltered(string.Format("workid_r in (select w2e.workid_r from wmswork2entity w2e " +
                                                            "join wmsiwb2cargo i2c on TO_CHAR(i2c.cargoiwbid_r) = w2e.work2entitykey " +
                                                            "join wmsiwbpos ip on ip.iwbid_r = i2c.iwbid_r where w2e.work2entityentity = 'CARGOIWB' " +
                                                            "and ip.iwbposid = {0})", _posID), GetModeEnum.Partial).ToList();
            }

            if (!workingList.Any()) 
                return new List<WorkingInfo>();

            var workIds = workingList.Where(p => p.WORKID_R.HasValue).Select(p => p.WORKID_R.Value).Distinct().ToArray();
            var works = new Dictionary<decimal, Work>();
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                foreach (var workid in workIds)
                {
                    works[workid] = mgr.Get(workid, GetModeEnum.Partial);
                }
            }
            return workingList.Where(p => p.WORKID_R.HasValue).Select(p => new WorkingInfo(p, works[p.WORKID_R.Value].Get<string>("VOPERATIONNAME"))).ToList();
        }

        protected override bool CanNew()
        {
            return IsEnabled && base.CanNew();
        }

        protected override bool CanEdit()
        {
            return IsEnabled && base.CanEdit();
        }

        protected override bool CanDelete()
        {
            return  IsEnabled && base.CanDelete();
        }

        private bool CanBeginWork(bool? parameter)
        {
           return true;
        }

        public void CheckIsEnabled()
        {
            Workings = RefreshWorking();

            if (!WMSEnvironment.Instance.WorkerId.HasValue)
                return;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                var res = mgr.GetFiltered(string.Format("WORKERID_R = {0} and WORKINGTILL is null and WORKID_R in (select wmswork.workid from wmswork where wmswork.OPERATIONCODE_R = '{1}')", WMSEnvironment.Instance.WorkerId.Value, BillOperationCode.OP_INPUT_REG.ToString())).ToList();
                if (res.Count() != 1) 
                    return;

                var dt = res.First().WORKINGFROM;
                StartWork(dt ?? DateTime.Now);
            }
        }
        
        private async void BeginWork(bool? isPushBtn)
        {
            string errMessage = null;
            //Проверяем наличие работника
            if (!WMSEnvironment.Instance.WorkerId.HasValue)
                errMessage = StringResources.WorkerNotSelect;
            if (_posID.HasValue)
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<CargoIWB>>())
                {
                    var res = mgr.GetFiltered(string.Format("cargoiwbid in (select i2c.cargoiwbid_r from wmsiwb2cargo i2c join wmsiwbpos ip on ip.iwbid_r = i2c.iwbid_r where ip.iwbposid = {0} )", _posID), GetModeEnum.Partial).ToList();
                    if (!res.Any()) 
                        errMessage = "Для накладной не указан входящий груз";
                }
            }
            else 
                errMessage = "Нет позиций у накладной";

            if (!string.IsNullOrEmpty(errMessage))
            {
                if (!isPushBtn.HasValue || isPushBtn.Value)
                    GetViewService().ShowDialog(StringResources.Error
                        , errMessage
                        , MessageBoxButton.OK
                        , MessageBoxImage.Warning
                        , MessageBoxResult.None);
                return;
            }

            if (!ConnectionManager.Instance.AllowRequest())
                return;

            var workerId = WMSEnvironment.Instance.WorkerId.Value;
            try
            {
                WaitStart();

                var result = await GetWorkAsync(workerId);
                if (!result)
                    return;

                StartWork(DateTime.Now);
            }
            finally
            {
                WaitStop();
            }
        }

        private void StartWork(DateTime dt)
        {
            IsEnabled = true;

            _beginCommandMenuItem.IsVisible = !IsEnabled;
            _endCommandMenuItem.IsVisible = IsEnabled;

            _startTime = dt;
            _timer.Start();
        }

        private async Task<bool> GetWorkAsync(decimal workerId)
        {
            var workOperation = BillOperationCode.OP_INPUT_REG.ToString();

            var workhelper = new WorkHelper();
            Func<DataRow[], string, string> dialogMessageHandler = (rows, workername) =>
            {
                return string.Format(StringResources.YouHaveWorkingsMessageFormat, Environment.NewLine,
                    string.Join(Environment.NewLine, rows.Select(p => string.Format("'{0}' ('{1}').", p["operationname"], p["workid"]))));
            };

            var result = workhelper.ClosingWorking(workerId: workerId, filter: null, dialogTitle: StringResources.Confirmation, workername: null, dialogMessageHandler: dialogMessageHandler);

            if (!result)
                return false;

            //Создаем работу
            List<Work> workList;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                var filter =
                    string.Format(
                        "operationcode_r = '{1}' and workid in (select w2e.workid_r from wmswork2entity w2e where w2e.work2entityentity = 'CARGOIWB' " +
                        "and w2e.work2entitykey in (select to_char(min(i2c.cargoiwbid_r)) from wmsiwb2cargo i2c " +
                        "left join wmsiwbpos ip on i2c.iwbid_r = ip.iwbid_r " +
                        "where ip.iwbposid ={0}))", _posID, workOperation);

                workList = mgr.GetFiltered(filter, GetModeEnum.Partial).ToList();
            }
            if (workList.Any())
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                {
                    var working = new Working
                    {
                        WORKID_R = workList.First().GetKey<decimal>(),
                        WORKERID_R = workerId,
                        WORKINGFROM = BPH.GetSystemDate()
                    };
                    mgr.Insert(ref working);
                }
            }
            else
            {
                List<CargoIWB> cargoIWBList;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<CargoIWB>>())
                {
                    cargoIWBList = mgr.GetFiltered(string.Format("cargoiwbid in (select min(i2c.cargoiwbid_r) from wmsiwb2cargo i2c join wmsiwbpos ip on ip.iwbid_r = i2c.iwbid_r where ip.iwbposid = {0} )",_posID), GetModeEnum.Partial).ToList();
                }
                if (cargoIWBList.Any())
                {
                    var mgrBpProcessManager = IoC.Instance.Resolve<IBPProcessManager>();
                    Work mywork;

                    mgrBpProcessManager.StartWorking("CARGOIWB", cargoIWBList.First().GetKey().ToString(), workOperation, workerId, cargoIWBList.First().MandantID, null, null, out mywork);
                }
            }
            
            Workings = RefreshWorking();
            return true;
        }
        
        private bool CanEndWork()
        {
            return true;
        }

        private void EndWork()
        {
            try
            {
                WaitStart();
                CloseWorkAsync();
                IsEnabled = false;
                TimeText = TimeSpan.Zero.ToString(@"mm\:ss");
                _timer.Stop();
            }
            finally
            {
                WaitStop();
            }
        }

        private async Task CloseWorkAsync()
        {
            List<Working> cargoIWBList;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                cargoIWBList = mgr.GetFiltered( string.Format("workerid_r = {0} and workingtill is null", WMSEnvironment.Instance.WorkerId.Value), GetModeEnum.Partial).ToList();
            }
            if (cargoIWBList.Any())
            {
                var mgrBpProcessManager = IoC.Instance.Resolve<IBPProcessManager>();
                mgrBpProcessManager.CompleteWorkings(cargoIWBList.Select(i => i.GetKey<Decimal>()), null);
            }

            Workings = RefreshWorking();
        }

        private ObservableCollection<DataField> GetSublistViewFields()
        {
            var fields = DataFieldHelper.Instance.GetDataFields(typeof(Working), SettingDisplay.List);
            fields.Add(new DataField
            {
                Name = InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                BindingPath = InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                SourceName = InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                FieldName = InputPlPosListViewModel.PlWorkingInfo.OperationPropertyName,
                FieldType = typeof(string),
                Caption = StringResources.Operation
            });
            return fields;
        }

        #endregion .  Methods  .
    }

    
    [SysObjectName("IWBPosInput")]
    public class IwbPosInputErrorInfo : IWBPosInput, IDXDataErrorInfo
    {
        public IwbPosInputErrorInfo()
        {
        }

        public IwbPosInputErrorInfo(IWBPosInput iwbPosInput)
        {
            if (iwbPosInput == null)
                return;

            try
            {
                SuspendNotifications();
                Copy(iwbPosInput, this);
                AcceptChanges();
            }
            finally
            {
                ResumeNotifications();
            }
        }

        void IDXDataErrorInfo.GetPropertyError(string propertyName, ErrorInfo info)
        {

        }

        void IDXDataErrorInfo.GetError(ErrorInfo info)
        {
            if (string.IsNullOrEmpty(BatchcodeErrorMessage))
            {
                SetErrorInfo(info, null, ErrorType.None);
                return;
            }

            SetErrorInfo(info, BatchcodeErrorMessage, string.IsNullOrEmpty(IWBPosInputBatch) ? ErrorType.Information : ErrorType.Critical);
        }

        private void SetErrorInfo(ErrorInfo info, string errorText, ErrorType errorType)
        {
            info.ErrorText = errorText;
            info.ErrorType = errorType;
        }
    }

    [SysObjectName("Working")]
    public class WorkingInfo : Working
    {
        public const string OperationPropertyName = "Operation";

        public WorkingInfo() { }

        public WorkingInfo(Working working, string operation)
        {
            if (working == null)
                return;

            try
            {
                SuspendNotifications();
                Copy(working, this);
                Operation = operation;
                AcceptChanges();
            }
            finally
            {
                ResumeNotifications();
            }
        }

        public string Operation { get; set; }
    }
}
