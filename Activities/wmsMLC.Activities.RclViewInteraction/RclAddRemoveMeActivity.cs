using System;
using System.Activities;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using log4net;
using wmsMLC.Activities.General.Helpers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;
using wmsMLC.General.PL.WPF.Services;

namespace wmsMLC.Activities.RclViewInteraction
{
    public class RclAddRemoveMeActivity : NativeActivity
    {
        #region .  Fields&Properties  .

        private NativeActivityContext _context;
        private Work CurrentWork { get; set; }

        private ILog _log = LogManager.GetLogger(typeof(RclShowWorkManageActivity));

        [DisplayName(@"Код работы")]
        public InArgument<decimal?> WorkId { get; set; }

        [DisplayName(@"Код склада")]
        [Description("Код склада используеся для фильтрации сотрудников и бригад в списках выбора")]
        public InArgument<string> WarehouseCode { get; set; }

        [DisplayName(@"Результат")]
        public OutArgument<bool> ResultOutArgument { get; set; }

        [DisplayName(@"Добавить/закрыть выполнение работ (True - добавить, False - закрыть)")]
        public InArgument<bool> OpenCloseInArgumentArgument { get; set; }

        [DisplayName(@"Размер шрифта")]
        [DefaultValue(14)]
        public InArgument<double> FontSize { get; set; }

        #endregion .  Fields&Properties  .

        public RclAddRemoveMeActivity()
        {
            DisplayName = "ТСД: Управление работой текущего пользователя";
            FontSize = 14;
        }

        protected override void Execute(NativeActivityContext context)
        {
            _context = context;
            ResultOutArgument.Set(context, true);

            var workId = WorkId.Get(_context);
            if (!workId.HasValue)
            {
                ShowMessage("Разработчику: В БП не указана работа");
                ResultOutArgument.Set(context, false);
                return;
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Work>>())
                CurrentWork = mgr.Get(workId.Value);

            if (CurrentWork == null)
            {
                ShowMessage("Разработчику: Не найдена работа с кодом " + workId.Value);
                ResultOutArgument.Set(context, false);
                return;
            }

           ResultOutArgument.Set(context, OpenCloseInArgumentArgument.Get(context) ? AddMe() : CloseMyWorkings());
        }

        #region .  Methods  .

        private bool AddMe()
        {
            var workerId = WMSEnvironment.Instance.WorkerId;
           
            try
            {
                if (!ValidateWorkingByWorkerId(workerId))
                    return false;

                if (!CheckMe(workerId))
                    return false;

                var w = new Working
                {
                    WORKERID_R = workerId,
                    WORKID_R = CurrentWork.GetKey<decimal>(),
                    WORKINGFROM = GetCorrectDate(),
                    TruckCode = WMSEnvironment.Instance.TruckCode,
                    WORKINGADDL = false
                };

                using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                    mgr.Insert(ref w);
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.GetErrorMessage(ex);
                _log.Warn(message, ex);

                ShowMessage(message, "Ошибка при добавлении сотрудника");
                return false;
            }
            return true;
        }

        private bool CloseMyWorkings()
        {
            var workerId = WMSEnvironment.Instance.WorkerId;

            if (!CheckMe(workerId))
                return false;

            var workerFilter = string.Format("{0} = {1} and {2} = {3} and {4} is null"
            , Working.WORKERID_RPropertyName
            , workerId
            , Working.WORKID_RPropertyName
            , CurrentWork.GetKey()
            , Working.WORKINGTILLPropertyName);

            try
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
                {
                    var workings = mgr.GetFiltered(workerFilter, GetModeEnum.Partial).ToArray();

                    foreach (var working in workings)
                    {
                        working.WORKINGTILL = GetCorrectDate();
                    }
                    mgr.Update(workings);
                }
                return true;
            }
            catch (Exception ex)
            {
                var message = ExceptionHelper.GetErrorMessage(ex);
                _log.Warn(message, ex);
                ShowMessage(message, "Ошибка при добавлении выполнения работ");

                return false;
            }
        }

        //Проверяем открытые работы для данного workerId
        private bool ValidateWorkingByWorkerId(decimal? workerId)
        {
            if (!workerId.HasValue)
                return false;

            var workhelper = new WorkHelper();
            var result = workhelper.ClosingWorking(workerId: workerId.Value, filter: null, dialogTitle: "Подтверждение",
                workername: null, dialogMessageHandler: ActivityHelpers.ClosingWorkingDialogMessage, dialogWorkerDateTillHandler: null, fontSize: FontSize.Get(_context));
            return result;
        }

        private bool CheckMe(decimal? workerId)
        {
            if (!workerId.HasValue)
            {
                ShowMessage(string.Format("К пользователю '{0}' не привязан работник. Невозможно создать выполнение работы", WMSEnvironment.Instance.AuthenticatedUser.GetSignature()));
                return false;
            }

            if (!OpenCloseInArgumentArgument.Get(_context))
                return true;

            var workerFilter = string.Format("{0} = {1} and {2} = {3} and {4} is null"
                , Working.WORKERID_RPropertyName
                , workerId.Value
                , Working.WORKID_RPropertyName
                , CurrentWork.GetKey()
                , Working.WORKINGTILLPropertyName);
            
            var warehouseCode = WarehouseCode.Get(_context);
            var filter = GetWorkersFilter(false); // в данном случае неважно, есть ли working
            filter += (!string.IsNullOrEmpty(filter) ? " and " : string.Empty) +
                      string.Format("(workerid = {0})", workerId);
            Worker[] items;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Worker>>())
                items = mgr.GetFiltered(filter, GetModeEnum.Partial).ToArray();
            if (items.Length == 0)
            {
                ShowMessage(
                    string.Format(
                        "Сотрудник (код '{0}') не существует или не привязан к складу (код '{1}') на даты работ",
                        workerId, warehouseCode));
                return false;
            }

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Working>>())
            {
                var workingItems = mgr.GetFiltered(workerFilter, GetModeEnum.Partial).ToArray();
                if (workingItems.Length > 0)
                {
                    var action = ShowMessage(string.Format("Сотрудник '{0}' уже выполняет работу (код '{1}'). Создать новую детализацию?", items.FirstOrDefault().WorkerFIO, CurrentWork.GetKey()), "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return (action == MessageBoxResult.Yes);
                }
            }

            return true;
        }

        private string GetWorkersFilter(bool freeOnly)
        {
            return RclShowWorkManageActivity.GetWorkersFilter(CurrentWork.GetKey<decimal?>(), WarehouseCode.Get(_context), freeOnly);
        }

        private static DateTime GetCorrectDate()
        {
            return BPH.GetSystemDate();
            //Убрал. иначе возникает ошибка при проверке на строне БД
            // убираем секунды, чтобы потом не мешали
            //var res = BPH.GetSystemDate();
            //res = res.AddSeconds(-1 * res.Second);
            //return res;
        }

        private MessageBoxResult ShowMessage(string message, string title = "Ошибка",
        MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Error,
        MessageBoxResult defaultButton = MessageBoxResult.OK)
        {
            var viewService = IoC.Instance.Resolve<IViewService>();
            return viewService.ShowDialog(title, message, buttons, image, defaultButton);
        }

        #endregion
    }
}