using System.Runtime.InteropServices;
using Microsoft.Practices.Unity;
using wmsMLC.Business.JabberManager;
using wmsMLC.Business.Managers;
using wmsMLC.DCL.Chat.ViewModels;
using wmsMLC.DCL.Main.ViewModels;
using wmsMLC.General.PL.WPF;

namespace wmsMLC.DCL.Chat
{
    public sealed class ChatModule : ModuleBase
    {
        #region .  Disable click sound  .

        const int FeatureDisableNavigationSounds = 21;
        const int SetFeatureOnProcess = 0x00000002;

        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int featureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable);

        static void DisableClickSounds()
        {
            CoInternetSetFeatureEnabled(
                FeatureDisableNavigationSounds,
                SetFeatureOnProcess,
                true);
        }
        #endregion

        public ChatModule(IUnityContainer container) : base(container)
        {
            DisableClickSounds();
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            //JabberManager
            Container.RegisterType<IChatManager, JabberManager>(new ContainerControlledLifetimeManager());
            //Chat
            Container.RegisterType<IChatViewModel, ChatViewModel>();
        }
    }
}
