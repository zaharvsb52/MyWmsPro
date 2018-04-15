using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Packing;
using wmsMLC.DCL.Packing.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.WebAndLoadTest
{
    [TestClass]
    public class PackingUnitTest : UnitTestWrapper
    {
        [TestMethod]
        public void InitPackingTest()
        {
            UnitTestHelper.Initialize(TestContext, typeof (PackingTest));
        }

        [TestMethod]
        public void TerminatePackingTest()
        {
            UnitTestHelper.Terminate(TestContext);
        }

        [TestMethod]
        public void PackingTest()
        {
            UnitTestHelper.Run(TestContext);
        }
    }

    public class PackingTest : UnitTestBase
    {
        private const string PackingPlaceParameterName = "PackingPlace";
        private const string PackingTeParameterName = "PackingTe";
        private const string PackingCountParameterName = "PackingCount";
        private string _packingPlace;
        private string _packingTe;
        private int _packingCount;
        private string[] _barcodes;

        private PackingViewModel _model;

        public override void Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            IoC.Instance.Register<IViewService, PackingViewService>();

            Assert.IsTrue(parameters.Contains(PackingPlaceParameterName));
            _packingPlace = (string)SerializationHelper.ConvertToTrueType(parameters[PackingPlaceParameterName], typeof (string));
            Assert.IsNotNull(_packingPlace);
            Assert.IsTrue(parameters.Contains(PackingTeParameterName));
            _packingTe = (string)SerializationHelper.ConvertToTrueType(parameters[PackingTeParameterName], typeof(string));
            Assert.IsTrue(parameters.Contains(PackingCountParameterName));
            _packingCount = (int)SerializationHelper.ConvertToTrueType(parameters[PackingCountParameterName], typeof(int));

            BPWorkflowManager.SetObjectCachable(PackWorkflowCodes.WfPack);

            _model = new PackingViewModel();

            // даем моделе время на инициализацию
            if (!new TaskFactory().StartNew(() =>
            {
                while (_model.AvailableTE == null || _model.PackingProducts == null)
                {
                    Thread.Sleep(1);
                }
            }).Wait(30000))
                throw new TimeoutException("Истекло время создания модели");

            if (!new TaskFactory().StartNew(() =>
            {
                _model.AvailableTE.Clear();
                _model.PackingProducts.Clear();
                _model.CurrentPlaceCode = null;
                _model.CurrentPlaceCode = _packingPlace;
                while (_model.AvailableTE.Count == 0 || _model.PackingProducts.Count == 0) { Thread.Sleep(1); }
            }).Wait(30000))
                throw new TimeoutException("Не удалось обновить данные");

            _model.VisiblePackingProducts = _model.PackingProducts;

            var packTE = _model.AvailableTE.FirstOrDefault(i => i.GetKey<string>() == _packingTe);
            Assert.IsNotNull(packTE);
            
            _model.SelectedPack = packTE;
            _model.SetActivePackCommand.Execute(null);

            _model.SelectedPackingProducts.Clear();
            _model.SelectedPackingProducts.Add(_model.PackingProducts.First());

            var filters = FilterHelper.GetArrayFilterIn("barcodekey", _model.PackingProducts.Select(i => i.SKUID.ToString()).Distinct().Cast<object>(), " and barcode2entity = 'SKU'");

            using (var mgr = IoC.Instance.Resolve<IBaseManager<Barcode>>())
                _barcodes = mgr.GetFiltered(string.Join(";", filters), GetModeEnum.Partial).Select(j => j.BarcodeValue).ToArray();
        }

        public override void Terminate()
        {
            try
            {
                // даем время допаковать все
                new TaskFactory().StartNew(() => { while (_model.PackInProgress) { Thread.Sleep(1); };}).Wait(10000);
                _model.SelectedPackedProducts.AddRange(_model.PackedProducts);
                //_model.ReturnOnSourceTE();
            }
            finally
            {
                // даем моделе время "остыть"
                new TaskFactory().StartNew(() => { while (true) { Thread.Sleep(1); } }).Wait(10000);
                base.Terminate();
            }
        }

        public override void Run()
        {
            var rnd = new Random();
            _model.PackingProductEditValue = _barcodes[rnd.Next(_barcodes.Length - 1)];

            while (_model.PackingProductEditValue != null || _model.PackInProgress)
            {
                Thread.Sleep(1);
            }
            if (!_model.PackStatus)
                Assert.Fail("Товар небыл упакован");
        }
    }

    public class PackingViewService : IViewService
    {
        public void AllRestoreLayout()
        {
            throw new NotImplementedException();
        }

        public bool ClearLayout(FrameworkElement obj, FormComponents formComponents, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            throw new NotImplementedException();
        }

        public bool? ClearLayout(IViewModel viewModel, string title, Type[] actionProcessingObjectTypes = null, Type[] doNotActionProcessingObjectTypes = null)
        {
            throw new NotImplementedException();
        }

        public bool Close(object viewOrViewModel, bool isDialog = false)
        {
            throw new NotImplementedException();
        }

        public bool CloseAll(bool absolutely)
        {
            throw new NotImplementedException();
        }

        public bool CloseAll()
        {
            throw new NotImplementedException();
        }

        public Window GetActiveWindow()
        {
            throw new NotImplementedException();
        }

        public void MakeMaxSize(IViewModel viewModel, bool isMax = true)
        {
            throw new NotImplementedException();
        }

        public void Register(string command, Type viewModelOrCommandType)
        {
            throw new NotImplementedException();
        }

        public IViewCommand ResolveViewCommand(string command)
        {
            throw new NotImplementedException();
        }

        public IViewModel ResolveViewModel(string command)
        {
            throw new NotImplementedException();
        }

        public void RestoreLayout(FrameworkElement obj)
        {
            throw new NotImplementedException();
        }

        public bool SaveDBLayout(FrameworkElement element, FormComponents formComponents, bool upCriticalVersion = false)
        {
            throw new NotImplementedException();
        }

        public bool? SaveDBLayout(IViewModel viewModel, string title, bool upCriticalVersion = false)
        {
            throw new NotImplementedException();
        }

        public bool SaveLayout(FrameworkElement element, FormComponents formComponents)
        {
            throw new NotImplementedException();
        }

        public bool? SaveLayout(IViewModel viewModel, string title)
        {
            throw new NotImplementedException();
        }

        public void Show(FrameworkElement content, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public void Show(FrameworkElement content)
        {
            throw new NotImplementedException();
        }

        public void Show(IView view, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public void AddViewToCache(Type modelType, IView view)
        {
            throw new NotImplementedException();
        }

        public void Show(IView view)
        {
            throw new NotImplementedException();
        }

        public void Show(IViewModel viewModel, ShowContext context, ref IViewModel currentViewModel)
        {
            throw new NotImplementedException();
        }

        public IView GetView(IViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public void Show(IViewModel viewModel, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public void Show(IViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public void Show(string command, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public void Show(string command)
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult ShowDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage image, MessageBoxResult defaultResult, double? fontSize = null)
        {
            return System.Windows.MessageBoxResult.OK;
        }

        public bool? ShowDialogWindow(IViewModel viewModel, bool isRestoredLayout, bool isNotNeededClosingOnOkResult = false, string width = null, string height = null, bool noButtons = false, MessageBoxButton buttons = MessageBoxButton.OKCancel, bool noActionOnCancelKey = false, IntPtr? intPtrOwner = null)
        {
            var expando = viewModel as ExpandoObjectViewModelBase;

            // TODO: возможно добавлять автозаполнение и других полей
            // количество для упаковки
            if (expando != null && expando.Fields.Any(i => i.Name == Product.ProductCountSKUPropertyName))
            {
                expando[Product.ProductCountSKUPropertyName] = 1;
            }
            return true;
        }

        public void ShowError(Exception ex, bool isExceptionStackIncluded)
        {
            throw new NotImplementedException();
        }

        public void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true)
        {
            throw new NotImplementedException();
        }

        public void ShowError(string message, Exception ex, string[] attachments, Dictionary<string, string> additionalParams, bool isBtnSandMailEnable = true, string mandantCode = null)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveViewCommand(string command, out IViewCommand viewCommand)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveViewModel(string command, out IViewModel viewModel)
        {
            throw new NotImplementedException();
        }
    }
}
