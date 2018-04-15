using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional
{
    [TestFixture, Ignore("Пока SDCL не будет запускаться на сервере - тесты не будут работать в автоматическом режиме")]
    public class TxmlListTest
    {
        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
//            BLHelper.RegisterServiceClient(Properties.Settings.Default.SessionId, Properties.Settings.Default.SDCL_Endpoint);
//            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
//            auth.Authenticate("DEBUG", "DEBUG");
        }

        [Test]
        public void SimpleManagerTest()
        {
            var pTask = new Task(() =>
            {
                Trace.WriteLine("Start partners");
                var mgr = new PartnerManager();
                var items = mgr.GetFiltered("ROWNUM < 500");
                Trace.WriteLine("Stop partners. Recieved " + items.Count());
            });

            pTask.Start();
            var uTask = new Task(() =>
            {
                Trace.WriteLine("Start users");
                var mgr = new UserManager();
                var items = mgr.GetFiltered("ROWNUM < 500", GetModeEnum.Full);
                Trace.WriteLine("Stop users. Recieved " + items.Count());
            });
            uTask.Start();

            Task.WaitAll(new[] { pTask, uTask });
        }

        [Test]
        public void ThreadGetAllTest()
        {
            // кэшируем
            var objs = IoC.Instance.Resolve<ISysObjectManager>().GetAll();
            //var users = new UserManager().GetUserRights("DEBUG");

            var tasks = new List<Task>();
//            var customTypes = new[] {typeof(SysObjectManager), typeof(ObjectTreeMenuManager), typeof(PartnerManager), typeof(MandantManager), typeof(TEManager), typeof(TETypeManager),
//                typeof(GlobalParamManager), typeof(TransportTaskStatusManager), typeof(UserManager), typeof(TransportTaskTypeManager), typeof(UserGroupManager), typeof(SequenceManager),
//                typeof(ProductBlockingManager), typeof(TE2BlockingManager), typeof(AreaManager), typeof(AreaTypeManager),  typeof(ObjectLookUpManager), typeof(PlaceClassManager),
//                typeof(RightManager), typeof(RightGroupManager), typeof(Right2GroupManager), typeof(User2GroupManager), typeof(Area2BlockingManager), typeof(UserGrp2RightGrpManager)};
            
            for (int i = 0; i < 1; i++)
            {
                for (int index = 0; index < BLHelper.Registered.Length; index++)
                {
                    var customType = BLHelper.Registered[index].ManagerType;
                    // для истории repoType = null
                    var repoType = BLHelper.Registered[index].OracleRepositoryType;
                    if (customType == null || repoType == null)
                        continue;

                    var task = new Task(() =>
                    {
                        try
                        {
                            var mgr = (IBaseManager)Activator.CreateInstance(customType);
                            Trace.WriteLine(Thread.CurrentThread.ManagedThreadId + ":Start " + customType.Name);
                            var startTime = DateTime.Now;
                            var items = mgr.GetFiltered("ROWNUM < 500");
                            Trace.WriteLine(string.Format(Thread.CurrentThread.ManagedThreadId + ":Stop {0}. Recieved {1} rows by {2}", customType.Name,
                                                          items.Count(), DateTime.Now - startTime));
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(string.Format(Thread.CurrentThread.ManagedThreadId + ":EXCEPTION {0}. {1}", customType.Name, ex));
                        }
                    });
                    task.Start();
                    tasks.Add(task);
                }
            }

            Task.WaitAll(tasks.ToArray());
        }

    }
}