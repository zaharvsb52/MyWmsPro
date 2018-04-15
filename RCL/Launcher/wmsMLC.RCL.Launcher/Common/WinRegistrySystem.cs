using Microsoft.Win32;

namespace wmsMLC.RCL.Launcher.Common
{
    public class WinRegistrySystem : WinRegistryBase
    {
        public WinRegistrySystem()
        {
            HivePath = "Ident";
        }

        public string Name { get; set; }

        protected override RegistryKey OpenKey()
        {
            return Registry.LocalMachine;
        }
    }
}
