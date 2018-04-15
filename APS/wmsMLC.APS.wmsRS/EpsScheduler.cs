using System;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using log4net;
using wmsMLC.General;

namespace wmsMLC.APS.wmsRS
{
    internal class EpsScheduler : IDisposable
    {
        #region .  Fields & Consts  .
        /// <summary>
        /// Константа, описывающая группу зданий для Quartz.
        /// </summary>
        public const string EpsGroup = "EPS";

        private readonly ILog _log = LogManager.GetLogger(typeof(EpsScheduler));
        private readonly IScheduler _sched;
        #endregion

        public EpsScheduler()
        {
            try
            {
                var factory = new StdSchedulerFactory();
                _sched = factory.GetScheduler();
                _sched.Start();
            }
            catch (Exception ex)
            {
                _log.Error("Scheduler start error", ex);
                throw;
            }
        }

        #region .  Methods  .
        /// <summary>
        /// Создание задания для запуска процесса.
        /// </summary>
        public bool ProcessJob(decimal? scheduleId, string scheduleCron, DateTime? from, DateTime? till)
        {
            try
            {
                if (string.IsNullOrEmpty(scheduleCron))
                    throw new DeveloperException("Cron parameter is not specified in schedule '{0}'", scheduleId);

                if (from.HasValue && till.HasValue && from.Value > till.Value)
                    throw new DeveloperException("Begin date is more then end date in schedule '{0}'", scheduleId);

                // получаем ключи
                var jobKey = new JobKey("Job" + scheduleId, EpsGroup);
                var trkey = new TriggerKey("Trigger" + scheduleId, EpsGroup);

                CronTriggerImpl trigger = null;

                // если такой триггер уже существует, снимаем с него параметры
                if (_sched.CheckExists(jobKey) && _sched.CheckExists(trkey))
                    trigger = (CronTriggerImpl)_sched.GetTrigger(trkey);

                if (Global.ValidateDateRange(GetNextFireTime(trigger), from, till))
                {
                    if (trigger == null)
                        CreateJob(CreateJobDataMap(scheduleId), jobKey, trkey, scheduleCron);
                    else
                        if (!trigger.CronExpressionString.EqIgnoreCase(scheduleCron, true)) //если задание существует сравниваем кроны
                        {
                            if (DeleteJob(scheduleId.Value))
                                CreateJob(CreateJobDataMap(scheduleId), jobKey, trkey, scheduleCron);
                            else
                                _log.DebugFormat("Can't change cron trigger for schedule {0}. Can't delete job. See previos error", scheduleId.Value);
                        }
                }
                else
                {
                    //удаляем задание.
                    DeleteJob(jobKey);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error processing job for schedule " + scheduleId, ex);
                return false;
            }
        }

        public bool DeleteJob(decimal scheduleId)
        {
            try
            {
                var jobKey = new JobKey("Job" + scheduleId, EpsGroup);
                DeleteJob(jobKey);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error in job delete for schedule " + scheduleId, ex);
                return false;
            }
        }

        private static JobDataMap CreateJobDataMap(object scheduleId)
        {
            return new JobDataMap { { Global.ScheduleId, scheduleId } };
        }

        private static DateTime GetNextFireTime(ITrigger trigger)
        {
            if (trigger == null)
                return DateTime.Now;

            var offset = trigger.GetNextFireTimeUtc();
            return offset.HasValue ? offset.Value.LocalDateTime : DateTime.Now;
        }

        private void CreateJob(JobDataMap jobDataMap, JobKey jobkey, TriggerKey trkey, string cronExpression)
        {
            var job = JobBuilder
                .Create<EpsSchedulerJob>()
                .WithIdentity(jobkey)
                .UsingJobData(jobDataMap)
                .Build();

            var trigger = (ICronTrigger)TriggerBuilder
                .Create()
                .WithIdentity(trkey)
                .WithCronSchedule(cronExpression)
                .Build();

            _sched.ScheduleJob(job, trigger);
            _log.InfoFormat("Create job '{0}' with cron '{1}' for Schedule '{2}'.", jobkey, cronExpression, jobDataMap[Global.ScheduleId]);
        }

        private void DeleteJob(JobKey jobKey)
        {
            if (!_sched.CheckExists(jobKey))
                return;

            _sched.DeleteJob(jobKey);
            _log.InfoFormat("Delete job '{0}'.", jobKey);
        }

        public void Dispose()
        {
            try
            {
                if (_sched != null)
                    _sched.Shutdown();
            }
            catch (Exception ex)
            {
                _log.Error("Shutdown error. " + ExceptionHelper.ExceptionToString(ex), ex);
            }
        } 
        #endregion
    }
}
