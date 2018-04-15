using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Components.ViewModels;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.WebAndLoadTest
{
     [TestClass]
    public class RclPackingUnitTest : UnitTestWrapper
    {
        [TestMethod]
        public void InitRclPackingTest()
        {
            UnitTestHelper.Initialize(TestContext, typeof(RclPackingTest));
        }

        [TestMethod]
        public void TerminateRclPackingTest()
        {
            UnitTestHelper.Terminate(TestContext);
        }

        [TestMethod]
        public void RclPackingTest()
        {
            UnitTestHelper.Run(TestContext);
        }
    }

    public class RclPackingTest : UnitTestBase
    {
        private const string TargetTeCodeParameterName = "TargetTeCode";
        private const string PackingPlaceParameterName = "PackingPlace";
        private const string CheckWhsParameterName = "CheckWhs";
        
        private string _packingPlaceCode;
        private string _targetTeCode;
        private bool _checkWhs;

        private Activity _rclPackingActivity;

        private BpContext _context;

        private TE[] _sourceTeList;

        private Dictionary<string, Barcode[]> _barcodes;

        private Random _random;

        private int _teCount;

        private TE _sourceTe;
        private TE _targetTe;

        public override void Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);

            IoC.Instance.Register<IViewService, RclPackingViewService>(LifeTime.Singleton);

            Assert.IsTrue(parameters.Contains(TargetTeCodeParameterName));
            _packingPlaceCode = (string)SerializationHelper.ConvertToTrueType(parameters[PackingPlaceParameterName], typeof(string));
            Assert.IsNotNull(_packingPlaceCode);
            Assert.IsTrue(parameters.Contains(TargetTeCodeParameterName));
            _targetTeCode = (string)SerializationHelper.ConvertToTrueType(parameters[TargetTeCodeParameterName], typeof(string));
            Assert.IsNotNull(_targetTeCode);
            _checkWhs = true;
            if (parameters.Contains(CheckWhsParameterName))
                _checkWhs = (bool)SerializationHelper.ConvertToTrueType(parameters[CheckWhsParameterName], typeof(bool));

            _rclPackingActivity = new RclPackingActivity();

            _barcodes = new Dictionary<string, Barcode[]>();

            using (var mgr = IoC.Instance.Resolve<IBaseManager<TE>>())
            {
                _sourceTeList =
                    mgr.GetFiltered(string.Format("tecurrentplace='{0}' and tepackstatus='TE_PKG_NONE'", _packingPlaceCode)).ToArray();
                _targetTe = mgr.Get(_targetTeCode);
            }
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Barcode>>())
            {
                foreach (var te in _sourceTeList)
                    _barcodes.Add(te.GetKey<string>(),
                        mgr.GetFiltered(string.Format("barcode2entity = 'SKU' and barcodekey in (select p.skuid_r from wmsproduct p where p.tecode_r = '{0}')", te.GetKey<string>())).ToArray());
            }

            _random = new Random();

            _teCount = _sourceTeList.Count();

            var vs = IoC.Instance.Resolve<IViewService>() as RclPackingViewService;
            if(vs == null)
                throw new Exception("IViewService is not RclPackingViewService");
            vs.Barcodes = _barcodes;
        }

        public override void Run()
        {
            var teIndex = _random.Next(_teCount);
            if (teIndex >= _teCount)
                teIndex = _teCount - 1;
            _sourceTe = _sourceTeList[teIndex];

            var vs = IoC.Instance.Resolve<IViewService>() as RclPackingViewService;
            if (vs == null)
                throw new Exception("IViewService is not RclPackingViewService");
            vs.CurrentTeCode = _sourceTe.GetKey<string>();

            _context = new BpContext();
            _context.Set<TE>("SOURCETE", _sourceTe);
            _context.Set<TE>("TARGETTE", _targetTe);
            _context.Set<bool>("CHECKWHS", _checkWhs);

            var inputs = new Dictionary<string, object> { { BpContext.BpContextArgumentName, _context } };
            var result = WorkflowInvoker.Invoke(_rclPackingActivity, inputs);
            var ctx = (BpContext)result[BpContext.BpContextArgumentName];
            Assert.IsTrue(ctx.Get<bool>("PACKED"));
        }
    }

    public class RclPackingViewService : IViewService
    {
        public Dictionary<string, Barcode[]> Barcodes { get; set; }

        public string CurrentTeCode { get; set; }

        public void Register(string command, Type viewModelOrCommandType)
        {
            throw new NotImplementedException();
        }

        public void Show(string command)
        {
            throw new NotImplementedException();
        }

        public void Show(string command, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public IViewModel ResolveViewModel(string command)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveViewModel(string command, out IViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public IViewCommand ResolveViewCommand(string command)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveViewCommand(string command, out IViewCommand viewCommand)
        {
            throw new NotImplementedException();
        }

        public void Show(IViewModel viewModel)
        {
        }

        public void Show(IViewModel viewModel, ShowContext context)
        {
        }

        public void Show(IViewModel viewModel, ShowContext context, ref IViewModel currentViewModel)
        {
        }

        public IView GetView(IViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public void AddViewToCache(Type modelType, IView view)
        {
            throw new NotImplementedException();
        }

        public void Show(IView view)
        {
        }

        public void Show(IView view, ShowContext context)
        {
        }

        public void Show(FrameworkElement content)
        {
            throw new NotImplementedException();
        }

        public void Show(FrameworkElement content, ShowContext context)
        {
            throw new NotImplementedException();
        }

        public bool? SaveLayout(IViewModel viewModel, string title)
        {
            throw new NotImplementedException();
        }

        public bool SaveLayout(FrameworkElement element, FormComponents formComponents)
        {
            throw new NotImplementedException();
        }

        public bool? SaveDBLayout(IViewModel viewModel, string title, bool upCriticalVersion = false)
        {
            throw new NotImplementedException();
        }

        public bool SaveDBLayout(FrameworkElement element, FormComponents formComponents, bool upCriticalVersion = false)
        {
            throw new NotImplementedException();
        }

        public void AllRestoreLayout()
        {
            throw new NotImplementedException();
        }

        public void RestoreLayout(FrameworkElement obj)
        {
            throw new NotImplementedException();
        }

        public bool? ClearLayout(IViewModel viewModel, string title, Type[] actionProcessingObjectTypes = null,
            Type[] doNotActionProcessingObjectTypes = null)
        {
            throw new NotImplementedException();
        }

        public bool ClearLayout(FrameworkElement obj, FormComponents formComponents, Type[] actionProcessingObjectTypes = null,
            Type[] doNotActionProcessingObjectTypes = null)
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult ShowDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage image,
            MessageBoxResult defaultResult, double? fontSize = null)
        {
            return MessageBoxResult.OK;
        }

        public bool? ShowDialogWindow(IViewModel viewModel, bool isRestoredLayout, bool isNotNeededClosingOnOkResult = false,
            string width = null, string height = null, bool noButtons = false, MessageBoxButton buttons = MessageBoxButton.OKCancel,
            bool noActionOnCancelKey = false, IntPtr? intPtrOwner = null)
        {
            var model = viewModel as DialogSourceViewModel;
            if (model == null)
                return true;
            var field = model.Fields.FirstOrDefault(f => f.SetFocus);
            if (field == null)
                return true;
            switch (field.Name)
            {
                case "count":
                    model["count"] = 1;
                    model.MenuCommand.Execute(new ValueDataField { Value = Key.Return});
                    break;
                case "sku":
                    var barcodes = Barcodes[CurrentTeCode];
                    var barcodeIndex = new Random().Next(barcodes.Count());
                    if (barcodeIndex >= barcodes.Count())
                        barcodeIndex = barcodes.Count() - 1;
                    model["sku"] = barcodes[barcodeIndex].BarcodeValue;
                    model.MenuCommand.Execute(new ValueDataField { Value = Key.Return });
                    break;
            }
            return true;
        }

        public bool Close(object viewOrViewModel, bool isDialog = false)
        {
            throw new NotImplementedException();
        }

        public bool CloseAll()
        {
            throw new NotImplementedException();
        }

        public bool CloseAll(bool absolutely)
        {
            throw new NotImplementedException();
        }

        public void MakeMaxSize(IViewModel viewModel, bool isMax = true)
        {
            throw new NotImplementedException();
        }

        public void ShowError(string message, Exception ex, string[] attachments, Dictionary<string, string> additionalParams,
            bool isBtnSandMailEnable = true, string mandantCode = null)
        {
        }

        public void ShowError(string message, Exception ex, bool isBtnSandMailEnable = true)
        {
        }

        public Window GetActiveWindow()
        {
            throw new NotImplementedException();
        }
    }
}
