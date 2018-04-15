using System;
using System.Threading.Tasks;
using System.Windows;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.WorkflowDesigner.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class WDUCLView : DXPanelView
    {
        private bool _initialized = false;

        public WDUCLView()
        {
            InitializeComponent();
            Loaded += WDUCLView_Loaded;
            DataContextChanged += WDUCLView_DataContextChanged;
        }

        void WDUCLView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as IObjectViewModel;
            if (vm != null)
            {
                vm.InitializeMenus();
            }
        }

        void WDUCLView_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
            {
                IWDUCLViewModelInternal vmInternal = null;
                Task.Factory.StartNew(() =>
                {
                    DispatcherHelper.Invoke(
                        new Func<IWDUCLViewModelInternal>(() => vmInternal = DataContext as IWDUCLViewModelInternal));
                    if (vmInternal != null)
                    {
                        vmInternal.StartWait();
                        var designerViewModel = vmInternal.GetDesignerViewModel();
                        var xamlViewModel = vmInternal.GetXamlViewModel();
                        DispatcherHelper.Invoke(
                            new Func<object>(
                                () => DesignerToolBox.DataContext = IoC.Instance.Resolve<IToolboxViewModel>()));
                        DispatcherHelper.Invoke(new Func<object>(() => Designer.DataContext = designerViewModel));
                        DispatcherHelper.Invoke(new Func<object>(() => PropertyInspector.DataContext = designerViewModel));
                        DispatcherHelper.Invoke(new Func<object>(() => XamlEditorView.DataContext = xamlViewModel));
                        _initialized = true;
                        DispatcherHelper.Invoke(new Action(() => vmInternal.LoadSource()));
                        vmInternal.StopWait();
                    }
                });
            }
        }
    }
}
