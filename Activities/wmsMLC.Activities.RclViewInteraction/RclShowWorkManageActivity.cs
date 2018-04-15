using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using log4net;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.Controls.Rcl;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclShowWorkManageActivity : NativeActivity
    {
        #region .  Fields  .
        private const string DefaultDateTimeFormat = "dd.MM.yy HH:mm:ss";
        private const string TitleError = "Ошибка";
        private const string WorkersModelFieldName = "lstWorkers";
        private const string WorkerBarCodeModelFieldName = "txtWorkerBC";

        //private const string DefaultWorkerTemplate =
        //    "FORMAT(\"{0} с {1:" + DefaultDateTimeFormat + "} по {2}\", vWorkerFIO, WORKINGFROM, IF(WORKINGTILL = null, \"н.в.\", FORMAT(\"{0:" + DefaultDateTimeFormat + "}\", WORKINGTILL)))";

        private ILog _log = LogManager.GetLogger(typeof(RclShowWorkManageActivity));

        private NativeActivityContext _context;
        private DialogSourceViewModel _mainModel;
        private Dictionary<decimal, Working> _workings;
        private ICryptoKeyProvider _cryptoKeyProvider;
        #endregion .  Fields  .

        #region .  Properties  .

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        [DisplayName(@"Код работы")]
        public InArgument<decimal?> WorkId { get; set; }

        [DisplayName(@"Код склада")]
        [Description("Код склада используеся для фильтрации сотрудников и бригад в списках выбора")]
        public InArgument<string> WarehouseCode { get; set; }

        [DisplayName(@"Пауза")]
        public InArgument<bool> IsOnPause { get; set; }

        [DisplayName(@"Открыть с")]
        public WorkManageActivityStartFrom StartsFrom { get; set; }

        [DisplayName(@"Назначенные работы")]
        public OutArgument<List<Working>> Workings { get; set; }

        private Work CurrentWork { get; set; }

        #endregion .  Properties  .

        public RclShowWorkManageActivity()
        {
            FontSize = 14;
            StartsFrom = WorkManageActivityStartFrom.Default;
            DisplayName = "ТСД: Управление работой";
        }

        #region .  Methods  .

        protected override void Execute(NativeActivityContext context)
        {
            _context = context;
            _workings = new Dictionary<decimal, Working>();

            var workId = WorkId.Get(_context);
            if (!workId.HasValue)
            {
                ShowMessage("Разработчику: В БП не указана работа");
                return;
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
                CurrentWork = mgr.Get(workId.Value);

            if (CurrentWork == null)
            {
                ShowMessage("Разработчику: Не найдена работа с кодом " + workId.Value);
                return;
            }

            StartFrom();

            Workings.Set(context, _workings.Where(p => !p.Value.WORKINGTILL.HasValue).Select(p => p.Value).ToList());
        }

        private void StartFrom()
        {
            switch (StartsFrom)
            {
                case WorkManageActivityStartFrom.Default:
                case WorkManageActivityStartFrom.ManageResources:
                    ShowManageResources();
                    break;

                case WorkManageActivityStartFrom.AddWorker:
                    ShowAddWorker();
                    break;

                case WorkManageActivityStartFrom.AddWorkerGroup:
                    ShowAddWorkerGroup();
                    break;

                case WorkManageActivityStartFrom.WorkingListEdit:
                    ShowWorkingListEditDialog();
                    break;

                case WorkManageActivityStartFrom.AddMe:
                    AddMe(true);
                    break;

                default:
                    throw new DeveloperException("Неизвестный начальный шаг " + StartsFrom);
            }
        }

        private DialogSourceViewModel GetManageResourcesModel()
        {
            var result = new DialogSourceViewModel
            {
                PanelCaption = "Инф. о работе",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var footerMenu = new List<ValueDataField>();
            var btnEditWork = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Редактировать",
                Value = "F4"
            };
            btnEditWork.Set(ValueDataFieldConstants.Row, 0);
            btnEditWork.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(btnEditWork);

            var btnDeleteWork = new ValueDataField
            {
                Name = "Menu1",
                Caption = "Удалить",
                Value = "F8"
            };
            btnDeleteWork.Set(ValueDataFieldConstants.Row, 0);
            btnDeleteWork.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(btnDeleteWork);

            var btnAddWorker = new ValueDataField
            {
                Name = "Menu2",
                Caption = "Доб. сотрудника",
                Value = "F2"
            };
            btnAddWorker.Set(ValueDataFieldConstants.Row, 1);
            btnAddWorker.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(btnAddWorker);

            var btnAddTeam = new ValueDataField
            {
                Name = "Menu3",
                Caption = "Доб. бригаду",
                Value = "F3"
            };
            btnAddTeam.Set(ValueDataFieldConstants.Row, 1);
            btnAddTeam.Set(ValueDataFieldConstants.Column, 1);
            footerMenu.Add(btnAddTeam);
            
            var field = new ValueDataField
            {
                Name = "txtWorkGroup",
                Caption = "Группа",
                FieldType = typeof(string),
                LabelPosition = "Left",
                IsEnabled = false,
                Value = string.Format("{0}", CurrentWork.WORKGROUPID_R)
            };
            result.Fields.Add(field);

            field = new ValueDataField
            {
                Name = WorkersModelFieldName,
                Caption = string.Empty,
                FieldType = typeof(DataTable),
                LabelPosition = "None",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true
            };
            field.FieldName = field.Name;
            field.SourceName = field.Name;

            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            field.Set(ValueDataFieldConstants.ShowControlMenu, true);
            field.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, true);
            field.Set(ValueDataFieldConstants.ShowAutoFilterRow, false);
            field.Set(ValueDataFieldConstants.DoNotActionOnEnterKey, true);
            field.Properties[ValueDataFieldConstants.FooterMenu] = footerMenu.ToArray();
            var works = GetWorksByGroup();
            field.Set(ValueDataFieldConstants.ItemsSource, works);

            var gridFields = new List<DataField>();
            var bestFitColumnNames = new List<string>();

            var gridField = new DataField
            {
                Name = "FIO",
                FieldType = typeof(string),
                Caption = "ФИО работника"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            gridFields.Add(gridField);

            gridField = new DataField
            {
                Name = "OPERATION",
                FieldType = typeof(string),
                Caption = "Операция"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            gridFields.Add(gridField);

            gridField = new DataField
            {
                Name = "WORKINGFROM",
                FieldType = typeof(DateTime),
                DisplayFormat = DefaultDateTimeFormat,
                Caption = "Дата с"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            //gridField.Properties[ValueDataFieldConstants.ColumnWidth] = 140;
            bestFitColumnNames.Add(gridField.FieldName);
            gridFields.Add(gridField);

            gridField = new DataField
            {
                Name = "WORKINGTILL",
                FieldType = typeof(DateTime),
                DisplayFormat = DefaultDateTimeFormat,
                Caption = "Дата по"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            //gridField.Properties[ValueDataFieldConstants.ColumnWidth] = 140;
            bestFitColumnNames.Add(gridField.FieldName);
            gridFields.Add(gridField);
            
            gridField = new DataField
            {
                Name = "WORKINGADDL",
                FieldType = typeof(bool),
                Caption = "Допол-ный"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            //gridField.Properties[ValueDataFieldConstants.ColumnWidth] = 55;
            bestFitColumnNames.Add(gridField.FieldName);
            gridFields.Add(gridField);
            
            gridField = new DataField
            {
                Name = "WORKID_R",
                FieldType = typeof(decimal),
                Caption = "Работа"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            //gridField.Properties[ValueDataFieldConstants.ColumnWidth] = 55;
            bestFitColumnNames.Add(gridField.FieldName);
            gridFields.Add(gridField);

            gridField = new DataField
            {
                Name = "WORKINGID",
                FieldType = typeof(decimal),
                Caption = "ID"
            };
            gridField.FieldName = gridField.Name;
            gridField.SourceName = gridField.Name;
            gridFields.Add(gridField);

            field.Set(ValueDataFieldConstants.Fields, gridFields.ToArray());
            field.Set(ValueDataFieldConstants.BestFitColumnNames, bestFitColumnNames.ToArray());
            result.Fields.Add(field);

            result.UpdateSource();
            return result;
        }

        private DataTable GetWorksByGroup()
        {
            var sql =
                "select wk.*, o.operationname as operation, trim(wkr.workerlastname||NVL2(wkr.workername,' '||wkr.workername, '')||NVL2(wkr.workermiddlename,' '||wkr.workermiddlename, '')) as FIO" +
                " from wmsworking wk" +
                " join wmswork w on wk.workid_r = w.workid" +
                " join billoperation o on w.operationcode_r = o.operationcode" +
                " join wmsworker wkr on wk.workerid_r = wkr.workerid" +
                " where " +
                (CurrentWork.WORKGROUPID_R.HasValue
                    ? string.Format("w.WORKGROUPID_R = {0}", CurrentWork.WORKGROUPID_R)
                    : string.Format("wk.workid_r = {0}", CurrentWork.GetKey()));

            using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
            {
                return manager.ExecuteDataTable(sql);
            }
        }
        
        private DialogSourceViewModel GetAddWorkerGroupModel()
        {
            var workerGroupModel = new DialogSourceViewModel
            {
                PanelCaption = "Добавление бригады",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var field1 = new ValueDataField();
            field1.Name = "txtGroupBC";
            field1.Caption = "ШК бриг.";
            field1.FieldName = field1.Name;
            field1.SourceName = field1.Name;
            field1.FieldType = typeof(string);
            field1.SetFocus = true;
            field1.CloseDialog = true;
            workerGroupModel.Fields.Add(field1);

            // фильтр отбора бригад по текущему складу время присутсвия которых попадает в диапазон времени работ
            var warehouseCode = WarehouseCode.Get(_context);

            var field2 = new ValueDataField();
            field2.Name = "lstGroup";
            field2.LabelPosition = "Top";
            field2.FieldName = field2.Name;
            field2.SourceName = field2.Name;
            field2.FieldType = typeof(Object);
            field2.Caption = string.Format("Список бригад (скл: '{0}')", string.IsNullOrEmpty(warehouseCode) ? "все склады" : warehouseCode);
            field2.LookupFilterExt = GetWorkerGroupFilter();
            field2.LookupCode = "WORKERGROUP_WORKERGROUPNAME_RCL";
            field2.Properties["MaxRowsOnPage"] = 6;
            field2.Properties["CloseDialogOnSelectedItemChanged"] = true;
            //field2.Properties["ParentKeyPreview"] = true;
            field2.Properties["LookupType"] = "SelectControl";
            workerGroupModel.Fields.Add(field2);

            workerGroupModel.UpdateSource();

            return workerGroupModel;
        }

        private DialogSourceViewModel GetAddWorkerModel()
        {
            var workerModel = new DialogSourceViewModel
            {
                PanelCaption = "Добавление сотрудника",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var txtWorkerBc = new ValueDataField {Name = WorkerBarCodeModelFieldName, Caption = "ШК сотрудника"};
            txtWorkerBc.FieldName = txtWorkerBc.Name;
            txtWorkerBc.SourceName = txtWorkerBc.Name;
            txtWorkerBc.FieldType = typeof(string);
            txtWorkerBc.SetFocus = true;
            txtWorkerBc.CloseDialog = true;
            workerModel.Fields.Add(txtWorkerBc);

            //Получаем данные
            var filter = GetWorkersFilter(false);
            filter += (!string.IsNullOrEmpty(filter) ? " AND " : string.Empty) +
                string.Format("WORKERID <> {0} AND WORKEREMPLOYEE = 1", WMSEnvironment.Instance.WorkerId);
            Worker[] itemsSource;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
            {
                itemsSource = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            }
            
            var lstWorkers = new ValueDataField
            {
                Name = WorkersModelFieldName,
                LabelPosition = "None",
                Caption = "Список сотрудников",
                FieldType = typeof(object),
            };
            lstWorkers.FieldName = lstWorkers.Name;
            lstWorkers.SourceName = lstWorkers.Name;
            lstWorkers.FieldType = typeof(object);
            lstWorkers.Properties[ValueDataFieldConstants.CloseDialogOnSelectedItemChanged] = true;
            lstWorkers.Properties[ValueDataFieldConstants.LookupType] = RclLookupType.SelectGridControl.ToString();
            lstWorkers.Properties[ValueDataFieldConstants.ValueMember] = "WORKERID";
            lstWorkers.Properties[ValueDataFieldConstants.DisplayMember] = "WORKERFIO";
            lstWorkers.Properties[ValueDataFieldConstants.ItemsSource] = itemsSource;
            lstWorkers.Set(ValueDataFieldConstants.ShowControlMenu, true);
            lstWorkers.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, true);
            lstWorkers.Set(ValueDataFieldConstants.ShowAutoFilterRow, true);
            workerModel.Fields.Add(lstWorkers);

            var fieldList = DataFieldHelper.Instance.GetDataFields(typeof(Worker), SettingDisplay.List);
            if (fieldList != null && fieldList.Any())
            {
                var gridFields = new List<DataField>();

                var gridField = fieldList.FirstOrDefault(p => p.FieldName == "WORKERFIO");
                if (gridField != null)
                    gridFields.Add(gridField);

                gridField = fieldList.FirstOrDefault(p => p.FieldName == "USERCODE_R");
                if (gridField != null)
                    gridFields.Add(gridField);

                var pk = new Worker().GetPrimaryKeyPropertyName();
                gridField = fieldList.FirstOrDefault(p => p.FieldName == pk);
                if (gridField != null)
                    gridFields.Add(gridField);

                if (gridFields.Any())
                    lstWorkers.Set(ValueDataFieldConstants.Fields, gridFields.ToArray());
            }

            workerModel.Fields.Add(lstWorkers);

            var footerMenuItem = new ValueDataField
            {
                Name = "btnEdit",
                Caption = "Нет ШК",
                FieldType = typeof(Button),
                Value = "F1"
            };
            footerMenuItem.FieldName = footerMenuItem.Name;
            footerMenuItem.SourceName = footerMenuItem.Name;

            var fieldFooterMenu = new ValueDataField { Name = "footerMenu" };
            fieldFooterMenu.FieldName = fieldFooterMenu.Name;
            fieldFooterMenu.SourceName = fieldFooterMenu.Name;
            fieldFooterMenu.FieldType = typeof(IFooterMenu);
            fieldFooterMenu.Properties[ValueDataFieldConstants.FooterMenu] = new[] {footerMenuItem};
            workerModel.Fields.Add(fieldFooterMenu);

            workerModel.UpdateSource();

            return workerModel;
        }

        private Working[] GetWorkersEditModelData()
        {
            string filter;
            if (CurrentWork.WORKGROUPID_R.HasValue)
            {
                filter = string.Format(
                    "wmsworking.workid_r in (select w.workid from wmswork w where w.workgroupid_r = {0})",
                        CurrentWork.WORKGROUPID_R);
            }
            else
            {
                filter = string.Format("WORKID_R = {0}", CurrentWork.GetKey());
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                return mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            }
        }

        private DialogSourceViewModel GetWorkersEditModel()
        {
            var workersEditModel = new DialogSourceViewModel
            {
                PanelCaption = "Редактирование сотрудников",
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var field1 = new ValueDataField
            {
                Name = "txtWorkGroup",
                Caption = "Группа",
                FieldType = typeof(string),
                LabelPosition = "Left",
                IsEnabled = false,
                Value = string.Format("{0}", CurrentWork.WORKGROUPID_R)
            };
            workersEditModel.Fields.Add(field1);

            var field2 = new ValueDataField
            {
                Name = WorkersModelFieldName,
                LabelPosition = "None",
                FieldType = typeof(object),
                SetFocus = true
            };
            field2.FieldName = field2.Name;
            field2.SourceName = field2.Name;

            //field2.LookupCode = "WORKING_WORKINGFIO_RCL";
            //field2.LookupFilterExt = string.Format("WORKID_R = {0}", CurrentWork.GetKey());
            //field2.Properties[ValueDataFieldConstants.MaxRowsOnPage] = 6;
            //field2.Properties[ValueDataFieldConstants.ParentKeyPreview] = true;
            //field2.Properties[ValueDataFieldConstants.CustomDisplayMember] = DefaultWorkerTemplate;

            field2.Set(ValueDataFieldConstants.CloseDialogOnSelectedItemChanged, true);
            field2.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            field2.Set(ValueDataFieldConstants.ShowControlMenu, false);
            field2.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, true);
            field2.Set(ValueDataFieldConstants.ShowAutoFilterRow, true);
            field2.Set(ValueDataFieldConstants.ValueMember, "WORKINGID");
            field2.Set(ValueDataFieldConstants.DisplayMember, "WORKERID_R");

            //Получим данные
            var itemsSource = GetWorkersEditModelData();
            field2.Set(ValueDataFieldConstants.ItemsSource, itemsSource);

            var bestFitColumnNames = new List<string>();
            var fieldList = DataFieldHelper.Instance.GetDataFields(typeof(Working), SettingDisplay.List);
            if (fieldList != null && fieldList.Any())
            {
                var gridFields = new List<DataField>();

                var gridField = fieldList.FirstOrDefault(p => p.FieldName == "VWORKERFIO");
                if (gridField != null)
                    gridFields.Add(gridField);

                gridField = fieldList.FirstOrDefault(p => p.FieldName == "WORKINGFROM");
                if (gridField != null)
                {
                    //gridField.Properties[ValueDataFieldConstants.ColumnWidth] = 140;
                    bestFitColumnNames.Add(gridField.FieldName);
                    gridFields.Add(gridField);
                }

                gridField = fieldList.FirstOrDefault(p => p.FieldName == "WORKINGTILL");
                if (gridField != null)
                {
                    //gridField.Properties[ValueDataFieldConstants.ColumnWidth] = 140;
                    bestFitColumnNames.Add(gridField.FieldName);
                    gridFields.Add(gridField);
                }

                gridField = fieldList.FirstOrDefault(p => p.FieldName == "WORKID_R");
                if (gridField != null)
                {
                    //gridField.Set(ValueDataFieldConstants.ColumnWidth, 55);
                    bestFitColumnNames.Add(gridField.FieldName);
                    gridField.LookupCode = null;
                    gridFields.Add(gridField);
                }

                var pk = new Working().GetPrimaryKeyPropertyName();
                gridField = fieldList.FirstOrDefault(p => p.FieldName == pk);
                if (gridField != null)
                    gridFields.Add(gridField);

                if (gridFields.Any())
                {
                    field2.Set(ValueDataFieldConstants.Fields, gridFields.ToArray());
                    field2.Set(ValueDataFieldConstants.BestFitColumnNames, bestFitColumnNames.ToArray());
                }
            }

            workersEditModel.Fields.Add(field2);

            var field3 = new ValueDataField
            {
                Name = "btnEdit",
                Caption = "Редакт."
            };
            field3.FieldName = field3.Name;
            field3.SourceName = field3.Name;
            field3.FieldType = typeof(Button);
            field3.SetFocus = true;
            field3.Value = "F4";

            var field4 = new ValueDataField
            {
                Name = "btnRemove",
                Caption = "Удалить"
            };
            field4.FieldName = field4.Name;
            field4.SourceName = field4.Name;
            field4.FieldType = typeof(Button);
            field4.SetFocus = false;
            field4.Value = "F8";

            field2.Properties[ValueDataFieldConstants.FooterMenu] = new[] { field3, field4 };

            workersEditModel.UpdateSource();

            return workersEditModel;
        }

        private DialogSourceViewModel GetWorkingEditModel(Working obj)
        {
            var workingModel = new DialogSourceViewModel
            {
                PanelCaption = string.Format("Выполнение № '{0}'", obj.GetKey()),
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var txtWorker = new ValueDataField
            {
                Name = "txtWorker",
                Caption = "Сотрудник"
            };
            txtWorker.FieldName = txtWorker.Name;
            txtWorker.SourceName = txtWorker.Name;
            txtWorker.FieldType = typeof(string);
            txtWorker.IsEnabled = false;
            txtWorker.SetFocus = true;
            txtWorker.Value = obj.VWorkerFIO;
            workingModel.Fields.Add(txtWorker);

            var field1 = new ValueDataField
            {
                Name = "dtDateFrom",
                Caption = "С"
            };
            field1.FieldName = field1.Name;
            field1.SourceName = field1.Name;
            field1.FieldType = typeof(DateTime);
            field1.DisplayFormat = DefaultDateTimeFormat;
            field1.SetFocus = true;
            field1.Value = obj.WORKINGFROM;
            workingModel.Fields.Add(field1);

            var field2 = new ValueDataField
            {
                Name = "dtDateTill",
                Caption = "По"
            };
            field2.FieldName = field2.Name;
            field2.SourceName = field2.Name;
            field2.FieldType = typeof(DateTime?);
            field2.DisplayFormat = DefaultDateTimeFormat;
            field2.Value = obj.WORKINGTILL;
            workingModel.Fields.Add(field2);

            var txtWorkingCount = new ValueDataField
            {
                Name = "txtWorkingCount",
                Caption = "Кол-во работ"
            };
            txtWorkingCount.FieldName = txtWorkingCount.Name;
            txtWorkingCount.SourceName = txtWorkingCount.Name;
            txtWorkingCount.FieldType = typeof(int?);
            //TODO: int format
            txtWorkingCount.Value = obj.WORKINGCOUNT;
            workingModel.Fields.Add(txtWorkingCount);

            var txtWorkingMult = new ValueDataField
            {
                Name = "txtWorkingMult",
                Caption = "Процент"
            };
            txtWorkingMult.FieldName = txtWorkingMult.Name;
            txtWorkingMult.SourceName = txtWorkingMult.Name;
            txtWorkingMult.FieldType = typeof(int);
            //TODO: int format
            txtWorkingMult.Value = obj.WORKINGMULT;
            workingModel.Fields.Add(txtWorkingMult);

            var txtWorkingAddl = new ValueDataField
            {
                Name = "txtWorkingAddl",
                Caption = "Допол-ный"
            };
            txtWorkingAddl.FieldName = txtWorkingAddl.Name;
            txtWorkingAddl.SourceName = txtWorkingAddl.Name;
            txtWorkingAddl.FieldType = typeof(bool);
            //TODO: bool format
            txtWorkingAddl.Value = obj.WORKINGADDL;
            workingModel.Fields.Add(txtWorkingAddl);

            var btnOk = new ValueDataField
            {
                Name = "btnOK",
                Caption = "Сохранить"
            };
            btnOk.FieldName = btnOk.Name;
            btnOk.SourceName = btnOk.Name;
            btnOk.FieldType = typeof(Button);
            btnOk.SetFocus = true;
            btnOk.Value = "F1";

            var btnCancel = new ValueDataField
            {
                Name = "btnCancel",
                Caption = "Отмена",
                FieldName = btnOk.Name,
                SourceName = btnOk.Name,
                FieldType = typeof (Button),
                SetFocus = false,
                Value = "Escape"
            };

            var btnFrom = new ValueDataField
            {
                Name = "btnFrom",
                Caption = "С"
            };
            btnFrom.FieldName = btnFrom.Name;
            btnFrom.SourceName = btnFrom.Name;
            btnFrom.FieldType = typeof(Button);
            btnFrom.SetFocus = false;
            btnFrom.Value = "F6";

            var btnTill = new ValueDataField
            {
                Name = "btnTill",
                Caption = "По"
            };
            btnTill.FieldName = btnTill.Name;
            btnTill.SourceName = btnTill.Name;
            btnTill.FieldType = typeof(Button);
            btnTill.SetFocus = false;
            btnTill.Value = "F7";

            var fieldFooterMenu = new ValueDataField {Name = "footerMenu"};
            fieldFooterMenu.FieldName = fieldFooterMenu.Name;
            fieldFooterMenu.SourceName = fieldFooterMenu.Name;
            fieldFooterMenu.FieldType = typeof(IFooterMenu);
            fieldFooterMenu.Properties["FooterMenu"] = new[] { btnOk, btnCancel, btnFrom, btnTill };
            workingModel.Fields.Add(fieldFooterMenu);

            workingModel.UpdateSource();

            return workingModel;
        }
      
        private void ShowManageResources()
        {
            var model = GetManageResourcesModel();
            //var viewService = IoC.Instance.Resolve<IViewService>();

            while (true)
            {
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    return;

                Working selectedWorking = null;
                if (menuResult == "1F4" || menuResult == "1Return" || menuResult == "1F8")
                {
                    var objId = model[WorkersModelFieldName];
                    decimal workingId;

                    if (!(objId is DataRowView))
                    {
                        ShowMessage("Не выбрано выполнение");
                        continue;
                    }
                    
                    if (!decimal.TryParse((objId as DataRowView)[0].ToString(), out workingId))
                    {
                        ShowMessage("Не удалось получить код выполнения работы ");
                        continue;
                    }

                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                        selectedWorking = mgr.Get(workingId);

                    if (selectedWorking == null)
                    {
                        ShowMessage("Не найдено выполнение работы с кодом " + workingId);
                        continue;
                    }
                }

                //отображаем меню выбора что делать
                switch (menuResult)
                {
                    // редактируем
                    case "1Return":
                    case "1F4":
                        //if (ShowWorkingEditDialog(selectedWorking))
                        //{
                            //Получим данные
                        //    var itemsSource = GetWorkersEditModelData();
                        //    model.GetField(WorkersModelFieldName).Set(ValueDataFieldConstants.ItemsSource, itemsSource);
                        //}
                        ShowWorkingEditDialog(selectedWorking);
                        break;

                    // удаляем
                    case "1F8":
                        using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                            mgr.Delete(selectedWorking);
                        break;

                    // добавить сотрудника
                    case "1F2":
                        ShowAddWorker(false);
                        break;

                    // добавить бригаду
                    case "1F3":
                        ShowAddWorkerGroup(false);
                        break;

                    default:
                        return;
                }

                var field = model.GetField(WorkersModelFieldName);
                field.Set(ValueDataFieldConstants.ItemsSource, GetWorksByGroup());

            }
        }

        private void ShowAddWorkerGroup(bool showEdit = true)
        {
            var model = GetAddWorkerGroupModel();
            while (true)
            {
                //отображаем меню выбора что делать
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    return;

                object workerGroup;
                switch (menuResult)
                {
                    // ввели со ШК
                    case "1":
                        workerGroup = model["txtGroupBC"];
                        break;

                    // выбрали в списке
                    case "1Value":
                        workerGroup = model["lstGroup"];
                        break;

                    default:
                        return;
                }

                if (workerGroup == null)
                {
                    ShowMessage("Бригада не выбрана");
                    continue;
                }

                decimal workerGroupId;
                if (!decimal.TryParse(workerGroup.ToString(), out workerGroupId))
                {
                    ShowMessage("Не удалось получить код бригады " + workerGroup);
                    continue;
                }

                var warehouseCode = WarehouseCode.Get(_context);
                var filter = GetWorkerGroupFilter();
                filter += (!string.IsNullOrEmpty(filter) ? " and " : string.Empty) +
                                string.Format("(workergroupid = {0})", workerGroupId);
                WorkerGroup[] items;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<WorkerGroup>>())
                    items = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
                if (items.Length == 0)
                {
                    ShowMessage(string.Format("Бригада (код '{0}') не существует или не привязана к складу (код '{1}') на даты работ", workerGroupId, warehouseCode));
                    return;
                }

                try
                {
                    using (var mgr = (IWorkManager)IoC.Instance.Resolve<IBaseManager<Work>>())
                        mgr.FillByGroup(CurrentWork.GetKey<decimal>(), workerGroupId);

                    // если все хорошо, то завершаем этот цикл
                    break;
                }
                catch (Exception ex)
                {
                    var message = ExceptionHelper.ExceptionToString(ex);

                    _log.Debug(ex);
                    _log.Warn(message);
                    ShowMessage(message, title: "Ошибка при добавлении бригады");
                }
            }

            if (showEdit) ShowWorkingListEditDialog();
        }

        private void ShowAddWorker(bool showEdit = true)
        {
            const string workerNotSelected = "Сотрудник не выбран";

            var model = GetAddWorkerModel();
            var workersField = model.GetField(WorkersModelFieldName);
            var itemsSource = workersField.Properties[ValueDataFieldConstants.ItemsSource];
            var valueMember = workersField.Properties[ValueDataFieldConstants.ValueMember];
            var displayMember = workersField.Properties[ValueDataFieldConstants.DisplayMember];

            var warehouseCode = WarehouseCode.Get(_context);

            while (true)
            {
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    return;

                //отображаем меню выбора что делать
                object worker;
                switch (menuResult)
                {
                    // ввели со ШК
                    case "1":
                        worker = model[WorkerBarCodeModelFieldName];
                        break;

                    // выбрали в списке
                    case "1Value":
                        worker = model[WorkersModelFieldName];
                        break;

                    // Выбор без ШК
                    case "1F1":
                        if (ShowSelectWorker("Добавление сотрудника", itemsSource, valueMember, displayMember, out worker))
                            break;
                        continue;

                    default:
                        return;
                }

                if (worker == null)
                {
                    ShowMessage(workerNotSelected);
                    continue;
                }

                Worker[] workers;

                //Предполагаем, что ввели USERCODE_R
                var svalue = worker.ToString();
                string barcode;
                var ucode = ParseBarcode(svalue, out barcode) ? barcode : svalue;
                model[WorkerBarCodeModelFieldName] = ucode;
                var filter = string.Format("USERCODE_R = '{0}'", ucode);
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
                    workers = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                switch (workers.Length)
                {
                    case 0:
                        //Вероятно ввели ид.
                        break;
                    case 1:
                        worker = workers[0].GetKey<decimal>();
                        break;
                    default:
                        if (ShowSelectWorker("Выберите сотрудника", workers, valueMember, displayMember, out worker))
                        {
                            if (worker == null)
                            {
                                ShowMessage(workerNotSelected);
                                continue;
                            }
                            break;
                        }
                        continue;
                }

                decimal workerId;
                if (!decimal.TryParse(worker.ToString(), out workerId))
                {
                    ShowMessage(string.Format("Не удалось получить код сотрудника '{0}'", worker));
                    continue;
                }

                filter = GetWorkersFilter(false);
                filter += (!string.IsNullOrEmpty(filter) ? " and " : string.Empty) +
                            string.Format("(workerid = {0})", workerId);

                using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
                    workers = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                if (workers.Length == 0)
                {
                    ShowMessage(string.Format("Сотрудник (код '{0}') не существует или не привязан к складу (код '{1}') на даты работ", workerId, warehouseCode));
                    continue;
                }

                if (!ValidateWorkingByWorkerId(workerId, workers[0].WorkerFIO))
                    continue;

                try
                {
                    var w = new Working
                    {
                        WORKERID_R = workerId,
                        WORKID_R = CurrentWork.GetKey<decimal>(),
                        WORKINGFROM = GetCorrectDate(),
                        WORKINGADDL = workerId != WMSEnvironment.Instance.WorkerId
                    };

                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                        mgr.Insert(ref w);

                    _workings[w.GetKey<decimal>()] = w;
                    // если все хорошо, то завершаем этот цикл
                    break;
                }
                catch (Exception ex)
                {
                    var message = ExceptionHelper.ExceptionToString(ex);

                    _log.Debug(ex);
                    _log.Warn(message);
                    ShowMessage(message, title: "Ошибка при добавлении сотрудника");
                }
            }

            if (showEdit) ShowWorkingListEditDialog();
        }

        private bool ParseBarcode(string code, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(code))
                return false;

            if (_cryptoKeyProvider == null)
                _cryptoKeyProvider = IoC.Instance.Resolve<ICryptoKeyProvider>();

            var descr = _cryptoKeyProvider.GetKey(0);
            var txt = CryptoHelper.Decrypt(code, descr);
            if (!string.IsNullOrEmpty(txt))
            {
                value = txt;
                return true;
            }
            return false;
        }

        private bool ShowSelectWorker(string dialogTitle, object itemsSource, object valueMember, object displayMember, out object worker)
        {
            worker = null;
            var model = GetSelectWorkerModel(dialogTitle, itemsSource, valueMember, displayMember);
            while (true)
            {
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    return false;

                //отображаем меню выбора что делать
                switch (menuResult)
                {
                    // выбрали в списке
                    case "1Value":
                        worker = model[WorkersModelFieldName];
                        return true;

                    default:
                        return false;
                }
            }
        }

        private DialogSourceViewModel GetSelectWorkerModel(string dialogTitle, object itemsSource, object valueMember, object displayMember)
        {
            var footerMenu = new List<ValueDataField>();
            var footerMenuItem = new ValueDataField
            {
                Name = "Menu0",
                Caption = "Отмена",
                Value = "Escape"
            };
            footerMenuItem.Set(ValueDataFieldConstants.Row, 0);
            footerMenuItem.Set(ValueDataFieldConstants.Column, 0);
            footerMenu.Add(footerMenuItem);

            var workerModel = new DialogSourceViewModel
            {
                PanelCaption = dialogTitle,
                FontSize = FontSize.Get(_context),
                IsMenuVisible = false,
            };

            var field = new ValueDataField
            {
                Name = WorkersModelFieldName,
                Caption = "Список сотрудников",
                FieldType = typeof(object),
                LabelPosition = "None",
                IsEnabled = true,
                SetFocus = true,
                CloseDialog = true,
            };
            field.Set(ValueDataFieldConstants.LookupType, RclLookupType.SelectGridControl.ToString());
            field.Set(ValueDataFieldConstants.ItemsSource, itemsSource);
            field.Set(ValueDataFieldConstants.ValueMember, valueMember);
            field.Set(ValueDataFieldConstants.DisplayMember, displayMember);
            field.Set(ValueDataFieldConstants.ShowControlMenu, true);
            field.Set(ValueDataFieldConstants.AllowAutoShowAutoFilterRow, true);
            field.Set(ValueDataFieldConstants.ShowAutoFilterRow, true);
            field.Properties[ValueDataFieldConstants.FooterMenu] = footerMenu.ToArray();

            var fieldList = DataFieldHelper.Instance.GetDataFields(typeof(Worker), SettingDisplay.List);
            if (fieldList != null && fieldList.Any())
            {
                var gridFields = new List<DataField>();

                var gridField = fieldList.FirstOrDefault(p => p.FieldName == "WORKERFIO");
                if (gridField != null)
                    gridFields.Add(gridField);

                gridField = fieldList.FirstOrDefault(p => p.FieldName == "USERCODE_R");
                if (gridField != null)
                    gridFields.Add(gridField);

                var pk = new Worker().GetPrimaryKeyPropertyName();
                gridField = fieldList.FirstOrDefault(p => p.FieldName == pk);
                if (gridField != null)
                    gridFields.Add(gridField);

                if (gridFields.Any())
                    field.Set(ValueDataFieldConstants.Fields, gridFields.ToArray());
            }

            workerModel.Fields.Add(field);

            workerModel.UpdateSource();

            return workerModel;
        }

        //Проверяем открытые работы для данного workerId
        private bool ValidateWorkingByWorkerId(decimal workerId, string workername)
        {
            var workhelper = new WorkHelper();
            var result = workhelper.ClosingWorking(workerId: workerId, filter: null, dialogTitle: "Подтверждение",
                workername: workername, dialogMessageHandler: ActivityHelpers.ClosingWorkingDialogMessage, dialogWorkerDateTillHandler: null, fontSize: FontSize.Get(_context));
            return result;
        }

        private void AddMe(bool simpleMode = false)
        {
            var workerId = WMSEnvironment.Instance.WorkerId;
            if (!workerId.HasValue)
                return;
            
            var warehouseCode = WarehouseCode.Get(_context);
            var filter = GetWorkersFilter(false); // в данном случае неважно, есть ли working
            filter += (!string.IsNullOrEmpty(filter) ? " and " : string.Empty) +
                            string.Format("(workerid = {0})", workerId);
            Worker[] items;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
                items = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            if (items.Length == 0)
            {
                var message = string.Format(
                    "Сотрудник (ид. '{0}') не существует или не привязан к складу (код '{1}') на даты работ",
                    workerId, warehouseCode);
                
                if (simpleMode)
                    throw new OperationException(message);
                ShowMessage(message);
                return;
            }

            try
            {
                if (!ValidateWorkingByWorkerId(workerId.Value, null))
                    return;

                Working w;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                {
                    var workerFilter = string.Format("{0} = {1} and {2} = {3} and {4} is null"
                        , Working.WORKERID_RPropertyName
                        , workerId.Value
                        , Working.WORKID_RPropertyName
                        , CurrentWork.GetKey()
                        , Working.WORKINGTILLPropertyName);

                    var workingItems = mgr.GetFiltered(workerFilter, GetModeEnum.Partial).ToArray();
                    if (workingItems.Length > 0)
                    {
                        var worker = items.First();
                        if(!simpleMode)
                            ShowMessage(string.Format("Сотрудник '{0}' уже выполняет работу (код '{1}')", worker.WorkerFIO, CurrentWork.GetKey()), title: "Внимание!", buttons: MessageBoxButton.OK, image: MessageBoxImage.Warning);
                        w = workingItems.First();
                    }
                    else
                    {
                        w = new Working
                        {
                            WORKERID_R = workerId,
                            WORKID_R = CurrentWork.GetKey<decimal>(),
                            WORKINGFROM = GetCorrectDate(),
                            TruckCode = WMSEnvironment.Instance.TruckCode,
                            WORKINGADDL = false
                        };

                        mgr.Insert(ref w);
                        _workings[w.GetKey<decimal>()] = w;
                    }
                }

                if (!simpleMode)
                    ShowWorkingEditDialog(w);
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.ExceptionToString(ex);

                _log.Debug(ex);
                _log.Warn(message);
                ShowMessage(message, title: "Ошибка при добавлении сотрудника");
            }
        }

        private void ShowWorkingListEditDialog()
        {
            var model = GetWorkersEditModel();

            while (true)
            {
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    return;

                var objId = model[WorkersModelFieldName];
                decimal workingId;
                if (objId == null || !decimal.TryParse(objId.ToString(), out workingId))
                {
                    ShowMessage("Не удалось получить код выполнения работы " + objId);
                    continue;
                }

                Working selectedWorking;
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                    selectedWorking = mgr.Get(workingId);

                if (selectedWorking == null)
                {
                    ShowMessage("Не найдено выполнение работы с кодом " + workingId);
                    continue;
                }

                //отображаем меню выбора что делать
                switch (menuResult)
                {
                    // редактируем
                    case "1F4":
                    case "1Value":
                        if (ShowWorkingEditDialog(selectedWorking))
                        {
                            //Получим данные
                            var itemsSource = GetWorkersEditModelData();
                            model.GetField(WorkersModelFieldName).Set(ValueDataFieldConstants.ItemsSource, itemsSource);
                        }
                        break;

                    // удаляем
                    case "1F8":
                        using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                            mgr.Delete(selectedWorking);
                        break;

                    default:
                        return;
                }
            }
        }

        private bool ShowWorkingEditDialog(Working working)
        {
            DialogSourceViewModel model = GetWorkingEditModel(working);
            //var fieldWorkingAddl = model.GetField(FieldNameWorkingAddl);
            //if (fieldWorkingAddl != null)
            //    fieldWorkingAddl.IsEnabled = WMSEnvironment.Instance.WorkerId != working.WORKERID_R;

            while (true)
            {
                string menuResult;
                if (RclShowDialogSourceActivity.ShowDialog(model, out menuResult) != true)
                    return false;

                switch (menuResult)
                {
                    case "1F1":
                        var dateFrom = (DateTime)model["dtDateFrom"];
                        var dateTill = model["dtDateTill"] == null ? null : (DateTime?)model["dtDateTill"];

                        if (!CheckWorkingDateFrom(dateFrom, working) || !CheckWorkingDateTill(dateTill))
                            continue;

                        // кол-во работ
                        working.WORKINGCOUNT = model["txtWorkingCount"] as decimal?;
                        // коэффициент
                        working.WORKINGMULT = (decimal)model["txtWorkingMult"];
                        // обновляем даты
                        working.WORKINGFROM = dateFrom;
                        working.WORKINGTILL = dateTill;
                        // допол.
                        working.WORKINGADDL = (bool)model["txtWorkingAddl"];

                        using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                            mgr.Update(working);
                        _workings[working.GetKey<decimal>()] = working;
                        return true;

                    case "1F6":
                        var newDateFrom = GetCorrectDate();
                        if (CheckWorkingDateFrom(newDateFrom, working))
                            model["dtDateFrom"] = GetCorrectDate();
                        break;

                    case "1F7":
                        var newDateTill = GetCorrectDate();
                        if (CheckWorkingDateTill(newDateTill))
                            model["dtDateTill"] = GetCorrectDate();
                        break;

                    default:
                        return false;
                }
            }
        }

        private bool CheckWorkingDateFrom(DateTime newDate, Working working)
        {
            var sysDate = GetCorrectDate();
            if (newDate > sysDate)
            {
                ShowMessage("Дата начала выполнения не может быть в будущем");
                return false;
            }

            //Не проверям диапазоны времени у Working'ов
            //if (newDate < CurrentWork.VWORKFROM || newDate > CurrentWork.VWORKTILL) 
            //{
            //    ShowMessage(string.Format("Дата начала выполнения должна быть в диапазоне дат работы ({0:" + DefaultDateTimeFormat + "}-{1:" + DefaultDateTimeFormat + "})",
            //        CurrentWork.VWORKFROM, CurrentWork.VWORKTILL));
            //    return false;
            //}

            if (newDate > working.WORKINGTILL)
            {
                ShowMessage("Дата начала выполнения не может быть больше даты его окончания");
                return false;
            }

            return true;
        }

        //private bool CheckWorkingDateTill(DateTime? newDate, Working working)
        private bool CheckWorkingDateTill(DateTime? newDate)
        {
            if (!newDate.HasValue)
                return true;

            var sysDate = GetCorrectDate();
            if (newDate > sysDate)
            {
                ShowMessage("Дата окончания выполнения не может быть в будущем");
                return false;
            }

            //if (newDate < CurrentWork.WORKFROM || newDate > CurrentWork.WORKTILL) gleb
            //{
            //    ShowMessage(string.Format("Дата начала выполнения должна быть в диапазоне дат работы ({0:" + DefaultDateTimeFormat + "}-{1:" + DefaultDateTimeFormat + "})",
            //        CurrentWork.WORKFROM, CurrentWork.WORKTILL));
            //    return false;
            //}

            //if (newDate < working.WORKINGFROM)
            //{
            //    ShowMessage("Дата окончания выполнения не может быть меньше даты его начала");
            //    return false;
            //}

            return true;
        }

        private void ShowMessage(string message, string title = TitleError, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Error, MessageBoxResult defaultButton = MessageBoxResult.OK)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            viewService.ShowDialog(title, message, buttons, image, defaultButton);
        }

        private static DateTime GetCorrectDate()
        {
            return BPH.GetSystemDate();
            //Ошибки на стороне БД
            // убираем секунды, чтобы потом не мешали
            //var res = BPH.GetSystemDate();
            //res = res.AddSeconds(-1 * res.Second);
            //return res;
        }

        private string GetWorkersFilter(bool freeOnly)
        {
            return GetWorkersFilter(CurrentWork.GetKey<decimal?>(), WarehouseCode.Get(_context), freeOnly);
        }

        internal static string GetWorkersFilter(decimal? workId, string warehouseCode,  bool freeOnly)
        {
            var res = freeOnly ? string.Format("{0} not in (select {1} from wmsworking where {2} = {3} and workingtill is null)",
                Worker.WorkerIDPropertyName
                , Working.WORKERID_RPropertyName
                , Working.WORKID_RPropertyName
                , workId) : "1=1";

            if (!string.IsNullOrEmpty(warehouseCode))
            {
                var datenow = GetCorrectDate();
                var whereClause = string.Format(" where w2w.warehousecode_r = '{0}'", warehouseCode) +
                    string.Format(" and not ('{0}' < w2w.worker2warehousefrom OR '{0}' > w2w.worker2warehousetill))",
                        SerializationHelper.GetCorrectStringValue(datenow));
                res += " and workerid in (select NVL(w2w.workerid_r, w2g.workerid_r) from wmsworker2warehouse w2w left join wmsworker2group w2g on w2g.workergroupid_r = w2w.workergroupid_r " + whereClause;
            }

            return res;
        }

        private string GetWorkerGroupFilter()
        {
            // фильтр отбора бригад по текущему складу время присутсвия которых попадает в диапазон времени работ
            var warehouseCode = WarehouseCode.Get(_context);

            if (string.IsNullOrEmpty(warehouseCode))
                return null;
            var dateTill = CurrentWork.VWORKTILL.HasValue ? CurrentWork.VWORKTILL.Value : GetCorrectDate();
            return "workergroupid in (select w2w.workergroupid_r from wmsworker2warehouse w2w where w2w.workergroupid_r is not null"
                                        + string.Format(" and w2w.warehousecode_r = '{0}'", warehouseCode)
                                        + string.Format(" and not (('{0}' < w2w.worker2warehousefrom and '{1}' < w2w.worker2warehousefrom) OR ('{0}' > w2w.worker2warehousetill and '{1}' > w2w.worker2warehousetill)))",
                                                SerializationHelper.GetCorrectStringValue(CurrentWork.VWORKFROM),
                                                SerializationHelper.GetCorrectStringValue(dateTill));
        }


        #endregion .  Methods  .
    }

    public enum WorkManageActivityStartFrom
    {
        Default,
        ManageResources,
        AddWorkerGroup,
        AddWorker,
        WorkingListEdit,
        AddMe
    }
}