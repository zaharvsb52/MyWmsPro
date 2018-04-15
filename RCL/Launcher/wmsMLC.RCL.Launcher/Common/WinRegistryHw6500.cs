using Microsoft.Win32;

namespace wmsMLC.RCL.Launcher.Common
{
    public class WinRegistryHw6500 : WinRegistryBase
    {
        public WinRegistryHw6500()
        {
            HivePath = @"System\Keypad";
        }

        public int RemapToVkF1 { get; set; }

        protected override RegistryKey OpenKey()
        {
            return Registry.LocalMachine;
        }
    }
}
