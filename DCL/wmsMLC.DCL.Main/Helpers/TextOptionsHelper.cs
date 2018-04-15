using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace wmsMLC.DCL.Main.Helpers
{
    public static class TextOptionsHelper 
    {
        public static void GetTextOptions(FrameworkElement element)
        {
            var ar = new[] {"Windows Vista", "Windows 7", "Windows 8"};
            if (ar.Any(j => j.Contains(GetOSName()))) return;
            RenderOptions.SetClearTypeHint(element, ClearTypeHint.Enabled);
            TextOptions.SetTextRenderingMode(element, TextRenderingMode.Aliased);
            TextOptions.SetTextFormattingMode(element, TextFormattingMode.Display);
        }

        private static string GetOSName()
        {
            var os = Environment.OSVersion;
            var osName = "Unknown";
            switch (os.Platform)
            {
                case PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0:
                            osName = "Windows 95";
                            break;
                        case 10:
                            osName = "Windows 98";
                            break;
                        case 90:
                            osName = "Windows ME";
                            break;
                    }
                    break;
                case PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 3:
                            osName = "Windws NT 3.51";
                            break;
                        case 4:
                            osName = "Windows NT 4";
                            break;
                        case 5:
                            switch (os.Version.Minor)
                            {
                                case 0:
                                    osName = "Windows 2000";
                                    break;
                                case 1:
                                    osName = "Windows XP";
                                    break;
                                case 2:
                                    osName = "Windows Server 2003";
                                    break;
                            }
                            break;
                        case 6:
                            osName = "Windows Vista";
                            switch (os.Version.Minor)
                            {
                                case 0:
                                    osName = "Windows Vista";
                                    break;
                                case 1:
                                    osName = "Windows 7";
                                    break;
                                case 2:
                                    osName = "Windows 8";
                                    break;
                            }
                            break;
                    }
                    break;
            }
            return osName;
        }

    }
}
