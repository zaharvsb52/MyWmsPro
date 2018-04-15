using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.Content.Views;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.BL.Validation;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(InputPlPosListView))]
    public class InputPlPosListViewModel : CustomObjectListViewModelBase<InputPlPos>, IDialogResultHandler, IValueEditController, IActionHandler
    {
        #region .  Consts  .
        public const string IsReadOnlyPropertyName = "IsReadOnly";
        private const string PlEntity = "PL";
        #endregion .  Consts  .

        #region . Fields  .
        private Exception _exception;
        private Working[] _openWorkings;
        private bool _isBpResultOk;
        private bool _workOk;
        private decimal? _plid;
        private string _warningMessage;
        private bool _useOneTypeTe;
        #endregion . Fields  .

        public InputPlPosListViewModel()
        {
            IsReadOnly = true;
            SublistViewItemType = typeof (Working);
            Works = new Dictionary<decimal, Work>();
            BeginWorkCommand = new DelegateCustomCommand(this, ObBeginWork, OnCanBeginWork);
            EndWorkCommand = new DelegateCustomCommand(this, OnEndWork, OnCanEndWork);
            DoActionCommand = new DelegateCustomCommand(this, OnOkClick, OnCanOkClick);
            OneTypeTeCommand = new DelegateCustomCommand<bool?>(this, OnOneTypeTe, CanOneTypeTe);
        }
        #region . Properties  .

        public string PositionCaption { get; set; }
        public bool? DialogResult { get; set; }
        public Dictionary<decimal, Work> Works { get; private set; }

        private List<PlWorkingInfo> _workings;
        public List<PlWorkingInfo> Workings
        {
            get { return _workings; }
            set
            {
                _workings = value;
                OnPropertyChanged("Workings");
            }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (_isReadOnly == value)
                    return;
                _isReadOnly = value;
                OnPropertyChanged(IsReadOnlyPropertyName);
            }
        }

        public Type SublistViewItemType { get; private set; }
        public string ActionWorkflowCode { get; set; }
        
        public ICommand BeginWorkCommand { get; private set; }
        public ICommand EndWorkCommand { get; private set; }
        public ICommand AppearanceStyleCommand { get; private set; }
        public ICommand DoActionCommand { get; private set; }
        public ICommand OneTypeTeCommand { get; private set; }
        #endregion . Properties  .

        #region .  Methods  .
        protected override void OnSourceChanged()
        {
            var validatable = Source as IValidatable;
            if (validatable != null)
                validatable.Validate();

            var eo = Source as IEditable;
            if (eo != null)
                OnIsDirtyChanged(eo);
        }

        protected override ObservableCollection<DataField> GetDataFields()
        {
            var result = base.GetDataFields();

            foreach (var dataField in result)
            {
                if (dataField.FieldName.EqIgnoreCase(InputPlPos.InputplposcountmanPropertyName)
                    || dataField.FieldName.EqIgnoreCase(InputPlPos.InputplpostemanPropertyName))
                {
                    dataField.IsEnabled = true;
                    dataField.EnableEdit = true;
                }
            }

            return result;
        }

        #region . Menu .

        protected override void CreateMainMenu()
        {
            var bar = Menu.GetOrCreateBarItem(StringResources.WorkCommands, 1);
            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.BeginWork,
                Command = BeginWorkCommand,
                ImageSmall = ImageResources.DCLWorkOpen16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkOpen32.GetBitmapImage(),
                //HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1
            });

            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.EndWork,
                Command = EndWorkCommand,
                ImageSmall = ImageResources.DCLWorkClose16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLWorkClose32.GetBitmapImage(),
                //HotKey = new KeyGesture(Key.F7),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 2
            });

            bar.MenuItems.Add(new SeparatorMenuItem { Priority = 3 });
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

        #endregion . Menu .

        public override async void RefreshData()
        {
            try
            {
                WaitStart();
                Workings = await OnRefreshSublistViewDataAsync();
            }
            finally
            {
                WaitStop();
            }
        }

        private Working[] GetWorkingsByOperations(string[] operations, string filter)
        {
            if (!HasPl())
                return null;

            const string workingFilter = "workingid in (" +
                "select distinct wkng.WorkingID from wmsWorking wkng" +
                " join wmsWork wrk on wrk.workid = wkng.workid_r" +
                " join wmsWork2Entity w2e on w2e.workid_r = wrk.workid" +
                " left join wmsW2E2Working w2e2w on w2e2w.workingid_r = wkng.workingid" +
                " where {0}" +
                " and w2e.work2entityentity = '{1}' and w2e.work2entitykey = {2} and (w2e2w.Work2EntityID_r = w2e.Work2EntityID or w2e2w.Work2EntityID_r is null){3})";

            string opfilter;
            if (operations == null || operations.Length == 0)
            {
                opfilter = null;
            }
            else if (operations.Length == 1)
            {
                opfilter = string.Format("wrk.operationcode_r = '{0}'", operations[0]);
            }
            else
            {
                opfilter = FilterHelper.GetFilterIn("wrk.operationcode_r", operations);
            }
            var whereclause = string.Format(workingFilter, opfilter, PlEntity, GetPlId(), filter);
            Working[] workings;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                workings = mgr.GetFiltered(whereclause, GetModeEnum.Partial).ToArray();
            }

            return workings.Where(p => p.WORKID_R.HasValue).ToArray();
        }

        private List<PlWorkingInfo> OnRefreshSublistViewData()
        {
            var operations = GetDefaultOperations();
            Works.Clear();
            var workings = GetWorkingsByOperations(operations, null);
            if (workings.Length == 0)
                return new List<PlWorkingInfo>();

            var workIds = workings.Where(p => p.WORKID_R.HasValue).Select(p => p.WORKID_R.Value).Distinct().ToArray();

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                foreach (var workid in workIds)
                {
                    Works[workid] = mgr.Get(workid, GetModeEnum.Partial);
                }
            }

            return workings.Where(p => p.WORKID_R.HasValue)
                .Select(p => new PlWorkingInfo(p, Works[p.WORKID_R.Value].Get<string>("VOPERATIONNAME")))
                    .ToList();
        }

        private async Task<List<PlWorkingInfo>> OnRefreshSublistViewDataAsync()
        {
            return await Task.Factory.StartNew(() => OnRefreshSublistViewData());
        }

        bool IValueEditController.CanEdit()
        {
            return OnCanEdit();
        }

        public bool EnableEdit(object entity, string propertyName)
        {
            if (!OnCanEdit() || entity == null || string.IsNullOrEmpty(propertyName))
                return false;
            var inputPlPos = entity as InputPlPos;
            if (inputPlPos == null)
                throw new DeveloperException("Объект не является типом InputPlPos.");
            var statePlPos = inputPlPos.GetProperty<string>(InputPlPos.StatuscodePropertyname).ToUpper();
            return statePlPos == "PLPOS_CREATED" || statePlPos == "PLPOS_MISSED" || statePlPos == "PLPOS_PART_PICKED" ||
                statePlPos == "PLPOS_ACTIVATED";
        }

        private bool OnCanBeginWork()
        {
            return !WaitIndicatorVisible && IsReadOnly && HasPl();
        }

        private async void ObBeginWork()
        {
            if (!OnCanBeginWork())
                return;

            //Проверяем наличие работника
            if (!WMSEnvironment.Instance.WorkerId.HasValue)
            {
                GetViewService().ShowDialog(StringResources.Error
                    , StringResources.WorkerNotSelect
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

                if (!string.IsNullOrEmpty(_warningMessage))
                {
                    GetViewService().ShowDialog(StringResources.Warning
                    , _warningMessage
                    , MessageBoxButton.OK
                    , MessageBoxImage.Asterisk
                    , MessageBoxResult.None);
                }

                //Получаем список выполнения работ для списка пикинга
                Workings = await OnRefreshSublistViewDataAsync();
                IsReadOnly = false;
                _workOk = true;
            }
            finally
            {
                _warningMessage = null;
                WaitStop();   
            }
        }

        private async Task<bool> GetWorkAsync(decimal workerId)
        {
            _warningMessage = null;
            if (!HasPl())
                return false;

            return await Task.Factory.StartNew(() =>
            {
                var workhelper = new WorkHelper();
                Func<DataRow[], string, string> dialogMessageHandler = (rows, workername) =>
                {
                    return string.Format(StringResources.YouHaveWorkingsMessageFormat, Environment.NewLine,
                        string.Join(Environment.NewLine, rows.Select(p => string.Format("'{0}' ('{1}').", p["operationname"], p["workid"]))));
                };

                var result = workhelper.ClosingWorking(workerId: workerId, filter: null, dialogTitle: StringResources.Confirmation, workername: null, dialogMessageHandler: dialogMessageHandler);
                if (!result)
                    return false;

                //Работа
                var workOperation = BillOperationCode.OP_PICK_END_MAN_REG.ToString();
                Work mywork;
                IBPProcessManager mgrBpProcessManager = null;
                IBaseManager<Work> mgrWork = null;

                try
                {
                    //Получим манданта
                    var skuid = Source.First().Get<decimal>("SKUID_R");
                    SKU sku;
                    using (var mgrsku = IoC.Instance.Resolve<IBaseManager<SKU>>())
                    {
                        sku = mgrsku.Get(skuid, GetModeEnum.Partial);
                    }
                    if (sku == null)
                        throw new DeveloperException("Can't find SKU.");

                    var mandantid = sku.Get<decimal>("MANDANTID");

                    mgrBpProcessManager = IoC.Instance.Resolve<IBPProcessManager>();
                    var splid = GetPlId().ToString(CultureInfo.InvariantCulture);
                    mgrBpProcessManager.StartWorking(PlEntity, splid, workOperation, workerId, mandantid, null, null, out mywork);

                    mgrWork = IoC.Instance.Resolve<IBaseManager<Work>>();
                    if (mywork == null)
                    {
                        mywork = mgrBpProcessManager.GetWorkByOperation(PlEntity, splid, workOperation);
                        if (mywork == null)
                            throw new DeveloperException("Work is not created.");
                    }
                    else
                    {
                        if (mywork.WORKINGL == null)
                            mywork = mgrWork.Get(mywork.GetKey<decimal>());
                    }

                    _openWorkings = null;
                    if (mywork.WORKINGL != null)
                        _openWorkings = mywork.WORKINGL.Where(p => p.WORKERID_R == workerId && !p.WORKINGTILL.HasValue).ToArray();

                    if (_openWorkings == null || _openWorkings.Length == 0)
                        throw new DeveloperException("Working is not created.");

                    //Проверяем группу работ
                    var workOp = GetWorksByOperations();
                    if (workOp != null && workOp.WORKGROUPID_R.HasValue &&
                        mywork.WORKGROUPID_R != workOp.WORKGROUPID_R)
                    {
                        mywork.WORKGROUPID_R = workOp.WORKGROUPID_R;
                        mgrWork.Update(mywork);
                    }

                    if (!mywork.WORKGROUPID_R.HasValue)
                    {
                        Mandant mandant;
                        using (var mgrmandant = IoC.Instance.Resolve<IBaseManager<Mandant>>())
                        {
                            mandant = mgrmandant.Get(mandantid, GetModeEnum.Partial);
                        }

                        _warningMessage = string.Format(StringResources.UndefinedWorkGroupForOperation,
                            workOperation,
                            (mandant == null ? mandantid.ToString(CultureInfo.InvariantCulture) : mandant.MandantCode));

                        //throw new DeveloperException(DeveloperExceptionResources.WorkGropUndefined);
                    }
                }
                finally 
                {
                    if (mgrBpProcessManager != null)
                        mgrBpProcessManager.Dispose();
                    if (mgrWork != null)
                        mgrWork.Dispose();
                }

                return true;
            });
        }

        private string[] GetDefaultOperations()
        {
            return new[]
            {
                BillOperationCode.OP_PICK_BEGIN.ToString(), 
                BillOperationCode.OP_PICK_BEGIN_MAN.ToString(),
                BillOperationCode.OP_PICK_END_MAN_REG.ToString()
            };
        }

        private Work GetWorksByOperations()
        {
            //Получаем все работы привязанные к данному списку пикинга
            Work[] works;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
            {
                works = mgr.GetFiltered(string.Format("workid in (" +
                    "select distinct w.workid from wmswork w" +
                    " join wmswork2entity on wmswork2entity.workid_r = w.workid" +
                    " where wmswork2entity.work2entityentity = '{0}'" +
                    " and {1}" +
                    " and wmswork2entity.work2entitykey = {2} and w.workgroupid_r is not null)",
                        PlEntity,
                        FilterHelper.GetFilterIn("w.operationcode_r", GetDefaultOperations()),
                        GetPlId()),
                        GetModeEnum.Partial).ToArray();
            }

            if (works.Length == 0)
                return null;

            if (works.Length == 1)
                return works[0];

            var result = works.FirstOrDefault(p => p.OPERATIONCODE_R == BillOperationCode.OP_PICK_BEGIN_MAN.ToString());
            if (result != null)
                return result;

            result = works.FirstOrDefault(p => p.OPERATIONCODE_R == BillOperationCode.OP_PICK_END_MAN_REG.ToString());
            if (result != null)
                return result;

            return works[0];
        }

        private bool OnCanEndWork()
        {
            return !WaitIndicatorVisible && !IsReadOnly;
        }

        private async void OnEndWork()
        {
            if (!OnCanEndWork())
                return;

            try
            {
                WaitStart();
                await CloseWorkingsAsync();

                //Получаем список выполнения работ для списка пикинга
                Workings = await OnRefreshSublistViewDataAsync();

                IsReadOnly = true;
            }
            finally
            {
                _openWorkings = null;
                WaitStop();
            }
        }

        private void CloseWorkings()
        {
            var ids = new List<decimal>();

            //Ищем открытые выполнения работ по операции OP_PICK_POS, OP_PICK_BEGIN, OP_PICK_BEGIN_MAN, OP_PICK_END_MAN_REG 
            var operations = new List<string>(GetDefaultOperations());
            operations.Add(BillOperationCode.OP_PICK_POS.ToString());
            var pickworkings = GetWorkingsByOperations(operations.ToArray(), " and wkng.WORKINGTILL is null");
            if (pickworkings.Any())
                ids.AddRange(pickworkings.Select(p => p.GetKey<decimal>()));
            if (_openWorkings != null && _openWorkings.Length > 0)
                ids.AddRange(_openWorkings.Select(p => p.GetKey<decimal>()));

            if (ids.Any())
            {
                //Закрываем выполнение работы
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    mgr.CompleteWorkings(ids.Distinct().ToArray(), null);
                }
            }
        }

        private async Task CloseWorkingsAsync()
        {
            await Task.Factory.StartNew(CloseWorkings);
        }

        private bool CanAppearanceStyle()
        {
            return IsCustomizeEnabled;
        }

        private void OnAppearanceStyle()
        {
            if (!CanAppearanceStyle())
                return;
            IsCustomization = false;
            IsCustomization = true;
        }

        private bool OnCanOkClick()
        {
            return (_workOk || OnCanEndWork()) && !_isBpResultOk;
        }

        //Обрабатываем ошибки после вызова DoAction
        private void OnOkClick()
        {
            if (_exception != null)
                throw _exception;
        }

        private bool OnCanEdit()
        {
            return !WaitIndicatorVisible && !IsReadOnly;
        }

        private bool CanOneTypeTe(bool? parameter)
        {
            return OnCanOkClick();
        }

        private void OnOneTypeTe(bool? parameter)
        {
            if (!CanOneTypeTe(parameter))
                return;
            _useOneTypeTe = parameter ?? false;
        }

        private bool HasPl()
        {
            var plpos = Source.FirstOrDefault();
            return plpos != null;
        }

        bool IActionHandler.DoAction()
        {
            DialogResult = null;
            _exception = null;

            if (!OnCanOkClick())
                return false;

            if (!ConnectionManager.Instance.AllowRequest())
                return false;

            try
            {
                WaitStart();
                var bpContext = new BpContext
                {
                    Items = Source.Cast<object>().ToArray(),
                    DoNotRefresh = true
                };
                bpContext.Set("UseOneTypeTe", _useOneTypeTe);

                var executionContext = new ExecutionContext(ActionWorkflowCode,
                    new Dictionary<string, object>
                    {
                        { BpContext.BpContextArgumentName, bpContext }
                    });
                var engine = IoC.Instance.Resolve<IProcessExecutorEngine>(WorkflowProcessExecutorConstants.Workflow);
                engine.Run(context: executionContext, completedHandler: CompletedBp);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                WaitStop();
                _exception = ex;
            }
            
            return false;
        }

        private void CompletedBp(CompleteContext ctx)
        {
            try
            {
                _isBpResultOk = false;
                if (ctx.Parameters != null && ctx.Parameters.ContainsKey(BpContext.BpContextArgumentName))
                {
                    const string isResultOk = "IsResultOk";
                    var bpcontext = ctx.Parameters[BpContext.BpContextArgumentName] as BpContext;
                    if (bpcontext != null)
                    {
                        _isBpResultOk = bpcontext.Get<bool>(isResultOk);
                        if (_isBpResultOk)
                        {
                            PLPos[] plposes;
                            using (var mgr = IoC.Instance.Resolve<IBaseManager<PLPos>>())
                            {
                                plposes = mgr.GetFiltered(string.Format("plid_r = {0} ", _plid), GetModeEnum.Partial).ToArray();
                            }

                            var inputPlPoses = new ObservableCollection<InputPlPos>();
                            foreach (var plpos in plposes)
                            {
                                var item = new InputPlPos(plpos);
                                inputPlPoses.Add(item);
                            }
                            ((IModelHandler) this).SetSource(inputPlPoses);

                            ViewService.ShowDialog(PanelCaption
                                , string.Format(StringResources.WfClosePickSuccessFormat, _plid)
                                , MessageBoxButton.OK
                                , MessageBoxImage.Asterisk, MessageBoxResult.OK);
                        }
                    }
                }
            }
            finally
            {
                WaitStop();
            }
        }

        protected override bool CanCloseInternal()
        {
            var result = base.CanCloseInternal();
            //if (result && !IsReadOnly)
            //{
            //    if (_myWorkings != null && _myWorkings.Any(p => !p.WORKINGTILL.HasValue))
            //    {
            //        ViewService.ShowDialog(PanelCaption
            //            , "Необходимо завершить работы, назначенные на Вас."
            //            , MessageBoxButton.OK
            //            , MessageBoxImage.Asterisk
            //            , MessageBoxResult.None);
            //        return false;
            //    }
            //}

            if (result && DialogResult != true && !_isBpResultOk)
            {
                var hasChanged = Source != null && Source.Any(p => p.IsDirty);
                if (hasChanged)
                {
                     var dr = ViewService.ShowDialog(StringResources.Confirmation
                    , string.Format(StringResources.CloseFromConfirmationFormat, Environment.NewLine)
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question
                    , MessageBoxResult.No);

                    result = dr == MessageBoxResult.Yes;
                }
            }

            if (result && !IsReadOnly)
            {
                try
                {
                    CloseWorkings();
                    Workings = OnRefreshSublistViewData();
                }
                catch (Exception ex)
                {
                    result = false;
                    ViewService.ShowError(null, ex);
                }
            }

            return result;
        }

        public ObservableCollection<DataField> GetSublistViewFields()
        {
            var fields = DataFieldHelper.Instance.GetDataFields(SublistViewItemType, SettingDisplay.List);
            fields.Add(new DataField
            {
                Name = PlWorkingInfo.OperationPropertyName,
                BindingPath = PlWorkingInfo.OperationPropertyName,
                SourceName = PlWorkingInfo.OperationPropertyName,
                FieldName = PlWorkingInfo.OperationPropertyName,
                FieldType = typeof(string),
                Caption = StringResources.Operation
            });

            var field = fields.FirstOrDefault(p => p.FieldName == Working.WORKID_RPropertyName);
            if (field != null)
                field.LookupCode = null;
           
            return fields;
        }

        public decimal GetPlId()
        {
            if (!HasPl())
                throw new DeveloperException("Id of PL is not defined.");

            if (!_plid.HasValue)
                _plid = Source.First().PlIdR;
            
            return _plid.Value;
        }
        #endregion .  Methods  .

        [SysObjectName("Working")]
        public class PlWorkingInfo : Working
        {
            public const string OperationPropertyName = "Operation";

            public PlWorkingInfo() { }

            public PlWorkingInfo(Working working, string operation)
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
}