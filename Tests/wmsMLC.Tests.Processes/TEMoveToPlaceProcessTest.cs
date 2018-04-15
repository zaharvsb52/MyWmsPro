using System.Linq;
using System.Threading;
using System.Windows;
using NUnit.Framework;
using Rhino.Mocks;
using wmsMLC.Business;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.Tests.Functional;
using wmsMLC.Tests.Functional.Entities;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    //[Ignore("InWork")]
    public class TEMoveToPlaceProcessTest
    {
        private const string ProcessCode = "TEMoveToPlace";
        private const string HeightWidth = "50%";

        [TestFixtureSetUp]
        public void Setup()
        {
            BLHelper.InitBL(null, DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
        }

        [Test]
        public void Process_Show_Error_If_Set_Zero_Items()
        {
            // MOCK-аем ViewService
            var viewService = MockRepository.GenerateStub<IViewService>();
            viewService.Stub(x => x
                            .ShowDialog(Arg<string>.Is.Anything,
                                Arg<string>.Is.Anything,
                                Arg<MessageBoxButton>.Is.Anything,
                                Arg<MessageBoxImage>.Is.Anything,
                                Arg<MessageBoxResult>.Is.Anything))
                            .Return(MessageBoxResult.OK);

            IViewService trueViewService;
            IoC.Instance.TryResolve(out trueViewService);
            try
            {
                // регистрируем в IoC
                IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                // запускаем процесс
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var manualEvent = new ManualResetEvent(false);
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext() { Items = new object[0] });
                    mgr.Run(ProcessCode, ctx => manualEvent.Set()); //Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>


                    // ожидаем когда выполнится
                    manualEvent.WaitOne();
                }

                // проверяемся, что была ошибка
                viewService.AssertWasCalled(x => x.ShowDialog(
                                        Arg<string>.Is.Equal("Ошибка"), // <- ожидаем сообщение с этим заголовком
                                        Arg<string>.Is.Anything,
                                        Arg<MessageBoxButton>.Is.Anything,
                                        Arg<MessageBoxImage>.Is.Anything,
                                        Arg<MessageBoxResult>.Is.Anything));
            }
            finally
            {
                // восстанавливаем нормальный ViewService
                if (trueViewService != null)
                    IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
            }
        }

        [Test]
        public void Process_Show_Error_If_Set_More_Than_One_Items()
        {
            // MOCK-аем ViewService
            var viewService = MockRepository.GenerateStub<IViewService>();
            viewService.Stub(x => x.ShowDialog(Arg<string>.Is.Anything,
                Arg<string>.Is.Anything,
                Arg<MessageBoxButton>.Is.Anything,
                Arg<MessageBoxImage>.Is.Anything,
                Arg<MessageBoxResult>.Is.Anything)).Return(MessageBoxResult.OK);

            IViewService trueViewService;
            IoC.Instance.TryResolve(out trueViewService);
            try
            {
                // регистрируем в IoC
                IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                // запускаем процесс
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var manualEvent = new ManualResetEvent(false);
                    var items = new object[] { new TE(), new TE() };
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext() { Items = items });
                    mgr.Run(ProcessCode, ctx => manualEvent.Set()); ////Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>

                    // ожидаем когда выполнится
                    manualEvent.WaitOne();
                }

                // проверяемся, что была ошибка
                viewService.AssertWasCalled(x => x.ShowDialog(
                    Arg<string>.Is.Equal("Ошибка"), // <- ожидаем сообщение с этим заголовком
                    Arg<string>.Is.Anything,
                    Arg<MessageBoxButton>.Is.Anything,
                    Arg<MessageBoxImage>.Is.Anything,
                    Arg<MessageBoxResult>.Is.Anything));
            }
            finally
            {
                // восстанавливаем нормальный ViewService
                if (trueViewService != null)
                    IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
            }
        }

        [Test]
        public void Process_Can_Be_Canceled_After_Setting_Strategy()
        {
            var te = new TE();

            // MOCK-аем ViewService
            var viewService = MockRepository.GenerateStub<IViewService>();

            // этот метод должен быть вызван для выбора правильной стратегии
            viewService.Stub(x => x.ShowDialogWindow(Arg<IViewModel>.Is.Anything, Arg<bool>.Is.Anything, Arg<bool>.Is.Anything, HeightWidth, HeightWidth))
                .WhenCalled(r =>
                {
                    var vm = (ExpandoObjectViewModelBase)r.Arguments[0];
                    // выставляем нужную стратегию
                    vm["strategy"] = "PLACE_FIX";
                })
                // отменяем дальнейшее выполнение
                .Return(false);

            // этот метод сообщит нам, что пользователь прервал выполнение
            viewService.Stub(x => x
                .ShowDialog(
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<MessageBoxButton>.Is.Anything,
                    Arg<MessageBoxImage>.Is.Anything,
                    Arg<MessageBoxResult>.Is.Anything))
                .Return(MessageBoxResult.OK);

            // запускаем
            IViewService trueViewService;
            IoC.Instance.TryResolve(out trueViewService);
            try
            {
                // регистрируем в IoC
                IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                // запускаем процесс
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var items = new object[] { te };
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext { Items = items });

                    var manualEvent = new ManualResetEvent(false);
                    mgr.Run(ProcessCode, ctx => { manualEvent.Set(); }); //Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>

                    // ожидаем когда выполнится
                    manualEvent.WaitOne();
                }

                // проверяемся, что была Отмена
                viewService.AssertWasCalled(x => x.ShowDialog(
                    Arg<string>.Is.Equal("Отмена процесса"), // <- ожидаем сообщение с этим заголовком
                    Arg<string>.Is.Anything,
                    Arg<MessageBoxButton>.Is.Anything,
                    Arg<MessageBoxImage>.Is.Anything,
                    Arg<MessageBoxResult>.Is.Anything));
            }
            finally
            {
                // восстанавливаем нормальный ViewService
                if (trueViewService != null)
                    IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
            }
        }

        [Test]
        public void Process_Show_Error_If_Strategy_Not_Set()
        {
            var te = new TE();

            // MOCK-аем ViewService
            var viewService = MockRepository.GenerateStub<IViewService>();

            // этот метод должен быть вызван для выбора правильной стратегии
            viewService.Stub(x => x
                        .ShowDialogWindow(
                            Arg<IViewModel>.Is.Anything,
                            Arg<bool>.Is.Anything,
                            Arg<bool>.Is.Anything,
                            HeightWidth,
                            HeightWidth))
                        .WhenCalled(r =>
                            {
                                // не выставляем стратегию
                                var vm = (ExpandoObjectViewModelBase)r.Arguments[0];
                                vm["strategy"] = null;
                            })
                // подтверждаем дальнейшее выполнение
                        .Return(true);

            // этот метод сообщит нам, об ошибке
            viewService.Stub(x => x
                        .ShowDialog(Arg<string>.Is.Anything,
                            Arg<string>.Is.Anything,
                            Arg<MessageBoxButton>.Is.Anything,
                            Arg<MessageBoxImage>.Is.Anything,
                            Arg<MessageBoxResult>.Is.Anything))
                        .Return(MessageBoxResult.OK);

            // запускаем
            IViewService trueViewService;
            IoC.Instance.TryResolve(out trueViewService);
            try
            {
                // регистрируем в IoC
                IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                // запускаем процесс
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var items = new object[] { te };
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext { Items = items });

                    var manualEvent = new ManualResetEvent(false);
                    mgr.Run(ProcessCode, ctx => manualEvent.Set()); //Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>

                    // ожидаем когда выполнится
                    manualEvent.WaitOne();
                }

                // выбор стратегии должен быть вызван
                viewService.AssertWasCalled(x => x.ShowDialogWindow(
                                            Arg<IViewModel>.Is.Anything,
                                            Arg<bool>.Is.Anything,
                                            Arg<bool>.Is.Anything,
                                            HeightWidth,
                                            HeightWidth));

                // проверяемся, что была ошибка
                viewService.AssertWasCalled(x => x.ShowDialog(
                                            Arg<string>.Is.Equal("Ошибка"), // <- ожидаем сообщение с этим заголовком
                                            Arg<string>.Is.Anything,
                                            Arg<MessageBoxButton>.Is.Anything,
                                            Arg<MessageBoxImage>.Is.Anything,
                                            Arg<MessageBoxResult>.Is.Anything));
            }
            finally
            {
                // восстанавливаем нормальный ViewService
                if (trueViewService != null)
                    IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
            }
        }

        [Test]
        public void Process_Show_Error_If_Strategy_Is_Unknown()
        {
            var te = new TE();

            // MOCK-аем ViewService
            var viewService = MockRepository.GenerateStub<IViewService>();

            // этот метод должен быть вызван для выбора правильной стратегии
            viewService.Stub(x => x
                        .ShowDialogWindow(
                            Arg<IViewModel>.Is.Anything,
                            Arg<bool>.Is.Anything,
                            Arg<bool>.Is.Anything,
                            HeightWidth,
                            HeightWidth))
                        .WhenCalled(r =>
                        {
                            // не выставляем стратегию
                            var vm = (ExpandoObjectViewModelBase)r.Arguments[0];
                            vm["strategy"] = "AQWQWESWDASDAD";
                        })
                // подтверждаем дальнейшее выполнение
                        .Return(true);

            // этот метод сообщит нам, об ошибке
            viewService.Stub(x => x
                        .ShowDialog(Arg<string>.Is.Anything,
                            Arg<string>.Is.Anything,
                            Arg<MessageBoxButton>.Is.Anything,
                            Arg<MessageBoxImage>.Is.Anything,
                            Arg<MessageBoxResult>.Is.Anything))
                        .Return(MessageBoxResult.OK);

            // запускаем
            IViewService trueViewService;
            IoC.Instance.TryResolve(out trueViewService);
            try
            {
                // регистрируем в IoC
                IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                // запускаем процесс
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var items = new object[] { te };
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext { Items = items });

                    var manualEvent = new ManualResetEvent(false);
                    mgr.Run(ProcessCode, ctx => manualEvent.Set()); //Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>

                    // ожидаем когда выполнится
                    manualEvent.WaitOne();
                }

                // выбор стратегии должен быть вызван
                viewService.AssertWasCalled(x => x.ShowDialogWindow(
                                            Arg<IViewModel>.Is.Anything,
                                            Arg<bool>.Is.Anything,
                                            Arg<bool>.Is.Anything,
                                            HeightWidth,
                                            HeightWidth));

                // проверяемся, что была ошибка
                viewService.AssertWasCalled(x => x.ShowDialog(
                                            Arg<string>.Is.Equal("Внимание"), // <- ожидаем сообщение с этим заголовком
                                            Arg<string>.Is.Anything,
                                            Arg<MessageBoxButton>.Is.Anything,
                                            Arg<MessageBoxImage>.Is.Anything,
                                            Arg<MessageBoxResult>.Is.Anything));
            }
            finally
            {
                // восстанавливаем нормальный ViewService
                if (trueViewService != null)
                    IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
            }
        }

        [Test]
        public void Process_Show_Warning_Message_If_Cant_Find_Places_For_Move()
        {
            // для несуществующего ТЕ и мест не будет
            var te = new TE();
            // код должен быть обязательно
            te.TECode = "test";

            // MOCK-аем ViewService
            var viewService = MockRepository.GenerateStub<IViewService>();

            // этот метод должен быть вызван для выбора правильной стратегии
            viewService.Stub(x => x
                        .ShowDialogWindow(
                            Arg<IViewModel>.Is.Anything,
                            Arg<bool>.Is.Anything,
                            Arg<bool>.Is.Anything,
                            HeightWidth,
                            HeightWidth))
                        .WhenCalled(r =>
                        {
                            // не выставляем стратегию
                            var vm = (ExpandoObjectViewModelBase)r.Arguments[0];
                            vm["strategy"] = "PLACE_FIX";
                        })
                // подтверждаем дальнейшее выполнение
                        .Return(true);

            // этот метод сообщит нам, об ошибке
            viewService.Stub(x => x
                        .ShowDialog(Arg<string>.Is.Anything,
                            Arg<string>.Is.Anything,
                            Arg<MessageBoxButton>.Is.Anything,
                            Arg<MessageBoxImage>.Is.Anything,
                            Arg<MessageBoxResult>.Is.Anything))
                        .Return(MessageBoxResult.OK);

            // запускаем
            IViewService trueViewService;
            IoC.Instance.TryResolve(out trueViewService);
            try
            {
                // регистрируем в IoC
                IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                // запускаем процесс
                using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                {
                    var items = new object[] { te };
                    mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext { Items = items });

                    var manualEvent = new ManualResetEvent(false);
                    mgr.Run(ProcessCode, ctx => manualEvent.Set()); //Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>

                    // ожидаем когда выполнится
                    manualEvent.WaitOne();
                }

                // выбор стратегии должен быть вызван
                viewService.AssertWasCalled(x => x.ShowDialogWindow(
                                            Arg<IViewModel>.Is.Anything,
                                            Arg<bool>.Is.Anything,
                                            Arg<bool>.Is.Anything,
                                            HeightWidth,
                                            HeightWidth));

                // проверяемся, что была ошибка
                viewService.AssertWasCalled(x => x.ShowDialog(
                                            Arg<string>.Is.Equal("Внимание"), // <- ожидаем сообщение с этим заголовком
                                            Arg<string>.Is.Anything,
                                            Arg<MessageBoxButton>.Is.Anything,
                                            Arg<MessageBoxImage>.Is.Anything,
                                            Arg<MessageBoxResult>.Is.Anything));
            }
            finally
            {
                // восстанавливаем нормальный ViewService
                if (trueViewService != null)
                    IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
            }
        }

        [Test]
        [Ignore("TODO")]
        public void Process_Can_Be_Canceled_After_Selecting_Place_For_Move()
        {
            var teTest = new TETest();
            var placeTest = new PlaceTest();
            try
            {
                // создаем миниатюрную ТЕ, чтобы точно влезла
                var te = teTest.CreateNew(t =>
                {
                    t.TEHeight = 1;
                    t.TELength = 1;
                    t.TEWidth = 1;
                    t.TEMaxWeight = 1;
                    t.TEWeight = 1;
                });

                var placeTo = placeTest.CreateNew();

                // MOCK-аем ViewService
                var viewService = MockRepository.GenerateStub<IViewService>();

                // этот метод должен быть вызван для выбора правильной стратегии
                viewService.Stub(x => x
                            .ShowDialogWindow(
                                Arg<IViewModel>.Is.Anything,
                                Arg<bool>.Is.Anything,
                                Arg<bool>.Is.Anything,
                                HeightWidth,
                                HeightWidth))
                            .WhenCalled(r =>
                            {
                                // не выставляем стратегию
                                var vm = (ExpandoObjectViewModelBase)r.Arguments[0];
                                vm["strategy"] = "PLACE_FIX";
                            })
                    // подтверждаем дальнейшее выполнение
                            .Return(true);

                // этот метод сообщит нам, об ошибке
                viewService.Stub(x => x
                            .ShowDialog(Arg<string>.Is.Anything,
                                Arg<string>.Is.Anything,
                                Arg<MessageBoxButton>.Is.Anything,
                                Arg<MessageBoxImage>.Is.Anything,
                                Arg<MessageBoxResult>.Is.Anything))
                            .Return(MessageBoxResult.OK);

                // запускаем
                IViewService trueViewService;
                IoC.Instance.TryResolve(out trueViewService);
                try
                {
                    // регистрируем в IoC
                    IoC.Instance.RegisterInstance(typeof(IViewService), viewService);

                    // запускаем процесс
                    using (var mgr = IoC.Instance.Resolve<IBPProcessManager>())
                    {
                        var items = new object[] { te };
                        mgr.Parameters.Add(BpContext.BpContextArgumentName, new BpContext { Items = items });

                        var manualEvent = new ManualResetEvent(false);
                        mgr.Run(ProcessCode, ctx => manualEvent.Set()); //Включение логгирования через  <appSettings><add key="NotUseActivityStackTrace" value="False"/><add key="UseWorkflowPersistAndLog" value="False"/></appSettings>

                        // ожидаем когда выполнится
                        manualEvent.WaitOne();
                    }

                    // выбор стратегии должен быть вызван
                    viewService.AssertWasCalled(x => x.ShowDialogWindow(
                                                Arg<IViewModel>.Is.Anything,
                                                Arg<bool>.Is.Anything,
                                                Arg<bool>.Is.Anything,
                                                HeightWidth,
                                                HeightWidth));

                    // проверяемся, что была ошибка
                    viewService.AssertWasCalled(x => x.ShowDialog(
                                                Arg<string>.Is.Equal("Внимание"), // <- ожидаем сообщение с этим заголовком
                                                Arg<string>.Is.Anything,
                                                Arg<MessageBoxButton>.Is.Anything,
                                                Arg<MessageBoxImage>.Is.Anything,
                                                Arg<MessageBoxResult>.Is.Anything));
                }
                finally
                {
                    // восстанавливаем нормальный ViewService
                    if (trueViewService != null)
                        IoC.Instance.RegisterInstance(typeof(IViewService), trueViewService);
                }
            }
            finally
            {
                teTest.ClearForSelf();
                placeTest.ClearForSelf();
            }
        }

        [Test]
        [Ignore("TODO")]
        public void Process_Create_TransportTask_If_All_Ok()
        {
        }
    }

    [TestFixture]
    public class CancelProductAcceptTest
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            BLHelper.InitBL(null, DALType.Oracle);
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
        }

        //[Test]
        public void TestListXmlParam()
        {
            using(var mgr = IoC.Instance.Resolve<IBaseManager<BPProcess>>())
            {
                var bpProcMgr = mgr as IBPProcessManager;
                var items = new decimal[7];
                bpProcMgr.CancelProductAccept(null, items, true, 0);
            }
        }
    }
}
