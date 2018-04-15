using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsRS
{
    internal class RsHost : AppHostBase
    {
        #region .  Fields  .
        private const int DefaultPeriodInSec = 60;
        private const int TimerTickPeriodNotify = 5;

        private const string TimerPeriodInSecSettingsName = "TimerPeriodInSec";

        private EpsScheduler _scheduler;
        private Timer _timer;
        private DateTime _date0;
        private long _totalTicks;

        private readonly long _timerPeriod;

        private DateTime? _lastCheckDate;
        #endregion

        public RsHost(ServiceContext context) : base(context)
        {
            // определяем период тиков
            _timerPeriod = GetPeriod();
        }

        #region .  Methods  .

        protected override void DoHostInternal(object context)
        {
            // выставим среду, если указана
            var env = Context.Get(ConfigBase.EnvironmentParam);
            ConfigurationManager.AppSettings["BLToolkit.DefaultConfiguration"] = string.IsNullOrEmpty(env) ? "DEV" : env;

            // явно инициализируем Oracle - чтобы не было ни каких накладок
            BLHelper.InitBL(dalType: DALType.Oracle);

            // аутентифицируемся
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(ConfigurationManager.AppSettings["Login"], ConfigurationManager.AppSettings["Password"]);

            // запрашиваем данные сразу
            BLHelper.FillInitialCaches();

            FreeScheduler();
            _scheduler = new EpsScheduler();

            FreeTimer();
            _timer = new Timer(Tick);

            _date0 = DateTime.Now;
            Log.Info("Service was initialized");

            _timer.Change(0, _timerPeriod); // стартуем сразу, а потом через период
            Log.Info("Service was started");
        }

        /// <summary>
        /// Событие таймера.
        /// </summary>
        private void Tick(object state)
        {
            Log.Debug("Timer tick");

            try
            {
                // останавливаем таймер
                _timer.Change(-1, -1);

                // освежаем счетчик
                _totalTicks++;
                var time = (DateTime.Now - _date0).TotalMinutes;
                if (time >= TimerTickPeriodNotify)
                {
                    Log.InfoFormat("I'm live. Total ticks are '{0}' per '{1:f1}' min.", _totalTicks, time);
                    _totalTicks = 0;
                    _date0 = DateTime.Now;
                }

                // если уже вычитывали - проверяем только изменения (по истории)
                HistoryWrapper<Schedule>[] scheduleHistoryList = null;
                if (_lastCheckDate != null)
                {
                    // вычитываем что поменялось с последней даты проверки
                    var hFilter = string.Format("{0} >= '{1:yyyyMMdd HH:mm:ss}'", HistoryWrapper<Schedule>.HDATEFROMPropertyName, _lastCheckDate.Value);
                    using (var manager = (IHistoryManager<Schedule>)IoC.Instance.Resolve<IBaseManager<Schedule>>())
                        scheduleHistoryList = manager.GetHistory(hFilter).ToArray();

                    // если ничего не изменилось
                    if (scheduleHistoryList.Length == 0)
                    {
                        Log.DebugFormat("Nothing has changed since '{0:yyyyMMdd HH:mm:ss}'", _lastCheckDate.Value);
                        return;
                    }

                    Log.DebugFormat("Find {0} changed item(s) since '{1:yyyyMMdd HH:mm:ss}'", scheduleHistoryList.Length, _lastCheckDate.Value);

                    // обновляем дату проверки
                    _lastCheckDate = GetSystemDate();

                    // побежали по истории
                    foreach (var changes in scheduleHistoryList.GroupBy(i => i.GetProperty(Schedule.ScheduleIdPropertyName)))
                    {
                        var items = changes.ToArray();
                        HistoryWrapper<Schedule> processingItem;

                        // после группировки или 1 или > 1
                        if (items.Length == 1)
                            processingItem = items[0];
                        else
                        {
                            // определям последнее изменение по максимальному Id
                            var maxHistId = changes.Max(i => i.GetProperty(HistoryWrapper<Schedule>.HISTORYIDPropertyName));
                            processingItem = changes.First(i => maxHistId.Equals(i.GetProperty(HistoryWrapper<Schedule>.HISTORYIDPropertyName)));
                        }

                        if (processingItem == null)
                            throw new DeveloperException("Can't find processing item for schedule " + changes.Key);

                        var scheduleId = processingItem.GetProperty<decimal?>(Schedule.ScheduleIdPropertyName);

                        // вычитываем транзакт
                        var transact = processingItem.GetProperty<decimal?>(Schedule.TransactPropertyName);

                        // удалили
                        if (transact == 0)
                            _scheduler.DeleteJob(scheduleId.Value);
                        // изменили
                        else
                        {
                            // запускаем
                            _scheduler.ProcessJob(scheduleId
                                , processingItem.GetProperty<string>(Schedule.ScheduleCronPropertyName)
                                , processingItem.GetProperty<DateTime?>(Schedule.ScheduleDateBeginPropertyName)
                                , processingItem.GetProperty<DateTime?>(Schedule.ScheduleDateEndPropertyName));
                        }

                    }
                }
                else // первый запуск
                {
                    _lastCheckDate = GetSystemDate();

                    // получаем активные на данный момент записи
                    var filter = string.Format("NVL({0}, SYSDATE-1) <= '{1:yyyyMMdd HH:mm:ss}' and '{1:yyyyMMdd HH:mm:ss}' < NVL({2}, SYSDATE+1)",
                        Schedule.ScheduleDateBeginPropertyName, _lastCheckDate, Schedule.ScheduleDateEndPropertyName);
                    Schedule[] scheduleList;
                    using (var manager = IoC.Instance.Resolve<IBaseManager<Schedule>>())
                        scheduleList = manager.GetFiltered(filter).ToArray();

                    if (scheduleList.Length == 0)
                    {
                        Log.Debug("Nothing to start. End tick");
                        return;
                    }

                    foreach (var schedule in scheduleList)
                        _scheduler.ProcessJob(schedule.ScheduleId, schedule.ScheduleCron, schedule.ScheduleDateBegin, schedule.ScheduleDateEnd);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Timer tick error " + ex.Message, ex);
            }
            finally
            {
                _timer.Change(_timerPeriod, _timerPeriod);
            }
        }

        private static DateTime GetSystemDate()
        {
            using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                return mgr.GetSystemDate();
        }

        private void FreeScheduler()
        {
            if (_scheduler == null)
                return;

            _scheduler.Dispose();
            _scheduler = null;
        }

        private void FreeTimer()
        {
            if (_timer == null)
                return;

            _timer.Change(-1, -1);
            _timer.Dispose();
            _timer = null;
        }

        private long GetPeriod()
        {
            var val = ConfigurationManager.AppSettings[TimerPeriodInSecSettingsName];
            var period = DefaultPeriodInSec;

            if (string.IsNullOrEmpty(val))
                Log.WarnFormat("Property {0} not set. System use default value {1}", TimerPeriodInSecSettingsName, period);
            else
            {
                if (!int.TryParse(val, out period))
                    Log.WarnFormat("Can't parse property {0} in settings. Value {1}. Property sets to default value", TimerPeriodInSecSettingsName, val);
            }

            // переводим в миллисекунды
            return period * 1000;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                    FreeTimer();

                if (_scheduler != null)
                    FreeScheduler();
            }

            base.Dispose(disposing);

            Log.DebugFormat("Service was uninitialized");
        }
        #endregion
    }
}