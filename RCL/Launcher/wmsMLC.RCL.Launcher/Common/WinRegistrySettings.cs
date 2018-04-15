namespace wmsMLC.RCL.Launcher.Common
{
    public class WinRegistrySettings : WinRegistryBase
    {
        public WinRegistrySettings()
        {
            HivePath = Global.HivePath;
        }

        public string Login { get; set; }

        //protected override RegistryKey OpenKey()
        //{
        //    return Registry.LocalMachine;
        //}
    }
}
