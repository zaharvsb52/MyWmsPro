using System.Windows.Input;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.DCL.Resources;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectListView))]
    public class PMListViewModel : ObjectListViewModelBase<PM>
    {
        public ICommand PmConfiguratorCommand { get; set; }

        private void OnShowPmConfigurator()
        {
            var vm = (IPmConfigViewModel)IoC.Instance.Resolve(typeof(IPmConfigViewModel));
            var vs = GetViewService();
            vm.Initialize();
            vs.ShowDialogWindow(viewModel: (IViewModel)vm, isRestoredLayout: true, width: "70%", height: "70%");
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();

            object vm;
            if (!IoC.Instance.TryResolve(typeof(IPmConfigViewModel), out vm))
                return;

            if (PmConfiguratorCommand == null)
                PmConfiguratorCommand = new DelegateCustomCommand(OnShowPmConfigurator);

            var barCommands = Menu.GetOrCreateBarItem(StringResources.Commands);
            barCommands.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.PmConfiguratorCaption,
                Hint = StringResources.PmConfiguratorHint,
                Command = PmConfiguratorCommand,
                ImageSmall = ImageResources.DCLPmConfigurator16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLPmConfigurator32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 502
            });
        }
    }
}
