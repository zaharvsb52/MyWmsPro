using log4net;
using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.Services.Service;

namespace wmsMLC.APS.wmsEPS
{
    public class EPSService : AppHostBase
    {
        #region .  Fields  .
        private readonly EpsConfig _config;
        private static IScheduler _scheduler;
        #endregion

        public EPSService(ServiceContext context) : base(context)
        {
            _config = new EpsConfig(context);
            ConfigurationManager.AppSettings["BLToolkit.DefaultConfiguration"] = _config.Environment;
        }

        #region .  Methods  .
        protected override void OnStop()
        {
            _scheduler.Shutdown();
            Log.Info("EPSService stopped");

            base.OnStop();
        }

        protected override void DoHostInternal(object context)
        {
            if (_config == null)
                throw new OperationException("Сервис не был сконфигурирован");

            BLHelper.InitBL(dalType: DALType.Oracle);
            Authenticate();
            BLHelper.FillInitialCaches();

            CreateReportDirectoryIfNotExists();
            CreateArchiveDirectoryIfNotExists();

            if (!EPS.wmsEPS.Properties.Settings.Default.UseLocalReport)
            {
                Log.Info("CheckOut reports to local directory");
                var checkOut = new EpsCheckOut();
                checkOut.CheckOut();
            }

            var schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler();
            _scheduler.Start();

            AddProcessOutputQueueJob();

            Log.Info("EPSService started");
        }

        private void Authenticate()
        {
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(ConfigurationManager.AppSettings["Login"], ConfigurationManager.AppSettings["Password"]);
        }

        public void AddProcessOutputQueueJob()
        {
            var jobKey = new JobKey("ProcessOutputQueueJob", "EPS");
            var trkey = new TriggerKey("ProcessOutputQueueTrigger", "EPS");

            var job = JobBuilder
                .Create<ProcessOutputQueueJob>()
                .WithIdentity(jobKey)
                .UsingJobData("handler", _config.Handler)
                .UsingJobData("batchSize", _config.BatchSize)
                .Build();

            var trigger = (ICronTrigger)TriggerBuilder
                .Create()
                .WithIdentity(trkey)
                .WithCronSchedule(_config.Cron)
                .Build();

            _scheduler.ScheduleJob(job, trigger);

            var nextFireTime = trigger.GetNextFireTimeUtc();
            if (nextFireTime != null)
                Log.Info("ProcessOutputQueueJob created. Next fire time: " + nextFireTime.Value.ToLocalTime());
            else
                Log.Error("ProcessOutputQueueJob created, but next fire time is null");
        }

        private void CreateReportDirectoryIfNotExists()
        {
            if (Directory.Exists(EPS.wmsEPS.Properties.Settings.Default.ReportPath))
                return;

            try
            {
                Directory.CreateDirectory(EPS.wmsEPS.Properties.Settings.Default.ReportPath);
            }
            catch (Exception ex)
            {
                Log.Error("Reports directory does not exists. Could not create.", ex);
                throw;
            }
        }

        private void CreateArchiveDirectoryIfNotExists()
        {
            if (Directory.Exists(EPS.wmsEPS.Properties.Settings.Default.ArchivePath))
                return;

            try
            {
                Directory.CreateDirectory(EPS.wmsEPS.Properties.Settings.Default.ArchivePath);
            }
            catch (Exception ex)
            {
                Log.Error("Archive directory does not exists. Could not create.", ex);
                throw;
            }
        } 
        #endregion

        internal class ProcessOutputQueueJob : IJob
        {
            private readonly ILog _log = LogManager.GetLogger(typeof (ProcessOutputQueueJob));

            public void Execute(IJobExecutionContext context)
            {
                try
                {
                    _log.Info("Job started. Next fire time: " + (context.NextFireTimeUtc.HasValue ? context.NextFireTimeUtc.Value.ToLocalTime().ToString() : "none"));
                    var handler = (int) context.JobDetail.JobDataMap["handler"];
                    var batchSize = (int) context.JobDetail.JobDataMap["batchSize"];
                    using (var mgr = (IOutputManager) IoC.Instance.Resolve<IBaseManager<Output>>())
                    {
                        var listEpsOutput = mgr.GetEpsOutputLst(batchSize, handler).ToArray();
                        if (listEpsOutput.Length > 0)
                        {
                            var tasks = listEpsOutput.Select(obj => Task.Factory.StartNew(() => mgr.PrintReport(obj))).Cast<Task>().ToArray();
                            Task.WaitAll(tasks);
                        }
                        else
                        {
                            _log.Info("No new tasks for handler " + handler);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Unhandled exception", ex);
                    throw new JobExecutionException("Unhandled exception", ex, false);
                }
            }
        }
    }
}