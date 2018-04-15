using System;
using log4net;
using Quartz;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.APS.wmsRS
{
    internal class EpsSchedulerJob : IJob
    {
        #region .  Consts & Fields  .
        //Используется для отладки
        //private static object _lock = new object();
        private readonly ILog _log = LogManager.GetLogger(typeof(EpsSchedulerJob)); 
        #endregion

        #region .  Methods  .
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                // проверяем параметры
                if (context == null || context.JobDetail == null || context.JobDetail.JobDataMap == null)
                    throw new DeveloperException("Job context is empty.");

                if (!context.JobDetail.JobDataMap.ContainsKey(Global.ScheduleId))
                    throw new DeveloperException("Job context does not contain a required parameter '{0}'.", Global.ScheduleId);

                //lock (_lock) //Используется для отладки
                {
                    var scheduleId = context.JobDetail.JobDataMap[Global.ScheduleId];
                    _log.InfoFormat("Execute Scheduler job '{0}'.", scheduleId);

                    // получаем расписание
                    var schedule = GetSchedule(scheduleId);

                    var userName = WMSEnvironment.Instance.AuthenticatedUser.GetSignature();
                    EpsJobExecutorHelper.RunJob(schedule.JobCode_R, string.Format("wmsMLC.RS.{0} ({1})", userName, schedule.GetKey()));
                }
            }
            catch (Exception ex)
            {
                _log.Error("Ошибка запуска задачи. " + ExceptionHelper.ExceptionToString(ex), ex);
            }
        }

        private static Schedule GetSchedule(object scheduleId)
        {
            Schedule res;

            //получаем данные по ключу из Schedule
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Schedule>>())
                res = mgr.Get(scheduleId);

            if (res == null)
                throw new DeveloperException("Can't get Schedule by key '{0}'.", scheduleId);

            //делаем проверку
            var now = DateTime.Now;
            if (!Global.ValidateDateRange(now, res.ScheduleDateBegin, res.ScheduleDateEnd))
                throw new DeveloperException("Date now '{0}' not in range: '{1}' '{2}'.", now, res.ScheduleDateBegin, res.ScheduleDateEnd);

            return res;
        }
        #endregion
    }
}
