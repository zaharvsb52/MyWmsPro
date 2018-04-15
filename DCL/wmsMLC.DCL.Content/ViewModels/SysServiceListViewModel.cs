using System.Linq;
using System.Windows.Input;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Views;
using wmsMLC.General.Services;
using wmsMLC.General.Types;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class SysServiceListViewModel : ObjectListViewModelBase<SysService>
    {
        public ICommand ChangeSDCLCommand { get; set; }

        private bool CanChange()
        {
            return SelectedItems.Count() == 1 && IsDelEnable && IsEditEnable && IsNewEnable;
        }

        private void ChangeSDCL()
        {
            if (!CanChange())
                return;

            var info = new SdclConnectInfo();

            var item = SelectedItems[0];
            var mgr = IoC.Instance.Resolve<IBaseManager<CustomParamValue>>();
            var ss = mgr.GetFiltered(string.Format("CUSTOMPARAMCODE_R='SSParEndPointL2' and CPVKEY='{0}'", item.GetKey())).ToArray();
            info.Code = item.ServiceCode;
            if (!ss.Any())
                throw new OperationException("Отсутствуют параметры подключения для сервиса '{0}'", item.GetKey());

            info.Endpoint = ss.First().CPVValue.To<string>();
            var sc = IoC.Instance.Resolve<IServiceClient>();
            sc.Reconnect(info);
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();

            ChangeSDCLCommand = new DelegateCustomCommand(ChangeSDCL, CanChange);
            Commands.Add(ChangeSDCLCommand);

            var barCommands = Menu.GetOrCreateBarItem(StringResources.Commands);
            barCommands.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Connect,
                Hint = StringResources.NextSDCL,
                Command = ChangeSDCLCommand,
                ImageSmall = ImageResources.DCLConnect16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLConnect32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1001
            });
        }
    }
}