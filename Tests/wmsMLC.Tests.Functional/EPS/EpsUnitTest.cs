using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional.EPS
{
    //[TestFixture, Ignore("Отдельное тестирование")]
    [TestFixture]
    public class EpsNUnitTest
    {
        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            //BLHelper.RegisterServiceClient(Functional.Properties.Settings.Default.SessionId, Functional.Properties.Settings.Default.SDCL_Endpoint);
            //var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            //auth.Authenticate("DEBUG", "DEBUG");
        }

        /// <summary>
        /// Данные берутся из БД.
        /// </summary>
        //[Test]
        public void StartJob()
        {
//            //Инициализируем сервис. Считываем конфигурационные файлы
//            var configManager = ConfigManagerBase.Instance<EpsConfigManager>(null);
//            configManager.SetDefaultValues();
//            configManager.OnEventErrorHandler += (exception, type, code, message, process, id, name, userName) =>
//                Trace.WriteLine(exception != null
//                    ? string.Format("Ошибка в EpsConfigManager'е. {0}", exception)
//                    : string.Format("Сообщение EpsConfigManager'а. {0}", process));
//
//            //TODO: переделать на контекст
//            //configManager.ReadConfig<EpsConfig>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPS"), "EPS.config");
//            var config = configManager.GetConfig() as EpsConfig;

            var ioutputmanager = CreateManager<Output>() as IOutputManager;
            if (ioutputmanager == null) throw new DeveloperException("Can't get IOutputManager.");
            //var listEpsOutput = ioutputmanager.GetEpsOutputLst(config.AmountQueue - job.Count, config.Handler);

            //TODO: переделать на контекст
            //config.Handler = 1;
            var listEpsOutput = ioutputmanager.GetEpsOutputLst(10, 1);

            if (listEpsOutput != null)
            {
                foreach (var output in listEpsOutput.Where(p => p != null))
                {
                    //Запускаем EPS Job
                    var epsjob = new wmsMLC.EPS.wmsEPS.EpsJob(output, "User");
                    var task = new Task(epsjob.DoJob);
                    task.Start();
                    Trace.WriteLine(string.Format("Задание с данными {0} запущено.", EpsHelper.GetKey(epsjob.GetOutput())));
                    Trace.WriteLine("======================================");
                    task.Wait();
                    using (var manager = IoC.Instance.Resolve<IBaseManager<Output>>())
                    {
                        manager.Update(output);
                    }
                }
            }
        }

        /// <summary>
        /// Данные формируются тестом EPSCreateDataTest.
        /// </summary>
        [Test]
        public void StartJobWithCreateData()
        {
            //добавляем данные в output
            var epsTest = new wmsMLC.Tests.Functional.EPS.EpsTest();
            var output = epsTest.CreateOutput();
            var key = ((IKeyHandler)output).GetKey();
            key.Should().NotBeNull("У сохраненного объекта Ключ должен быть заполнен.");

            //var mgr = IoC.Instance.Resolve<IBaseManager<Output>>();
            //mgr.Insert(ref output);

            //Инициализируем сервис. Считываем конфигурационные файлы
            //TODO: переделать на контекст
//            var configManager = ConfigManagerBase.Instance<EpsConfigManager>(new ServiceContext("StartJobWithCreateData", null));
//            configManager.SetDefaultValues();
//            configManager.OnEventErrorHandler += (exception, type, code, message, process, id, name, userName) => 
//                Trace.WriteLine(exception != null
//                    ? string.Format("Ошибка в EpsConfigManager'е. {0}", exception)
//                    : string.Format("Сообщение EpsConfigManager'а. {0}", process));
//
//
//            //TODO: переделать на контекст
//            //configManager.ReadConfig<EpsConfig>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EPS"), "EPS.config");
//            var config = configManager.GetConfig();

            //Запускаем EPS Job
            var epsjob = new wmsMLC.EPS.wmsEPS.EpsJob(output, "User");
            var task = new Task(epsjob.DoJob);
            task.Start();
            Trace.WriteLine(string.Format("Задание с данными {0} запущено.", EpsHelper.GetKey(epsjob.GetOutput())));
            Trace.WriteLine("======================================");
            task.Wait();
            using (var manager = IoC.Instance.Resolve<IBaseManager<Output>>())
            {
                manager.Update(output);
            }
        }

        private IBaseManager<T> CreateManager<T>()
        {
            return IoC.Instance.Resolve<IBaseManager<T>>();
        }
    }
}
