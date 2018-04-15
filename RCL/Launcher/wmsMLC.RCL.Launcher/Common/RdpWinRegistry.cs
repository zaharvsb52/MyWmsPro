using System.IO;

namespace wmsMLC.RCL.Launcher.Common
{
    //[HKEY_CURRENT_USER\Software\Motorola\MotoTscClient]
    public class RdpWinRegistry : WinRegistryBase
    {
        public RdpWinRegistry()
        {
            HivePath = @"Software\Motorola\MotoTscClient";
        }

        //"DebugLogFile"="\\Program Files\\MotoRdp\\MotoDbgLog.txt"
        public string DebugLogFile { get; set; }
        
        //"UnicodeTextFile"="\\Program Files\\MotoRdp\\MotoTscClientEnglishUTF.xml"
        public string UnicodeTextFile { get; set; }

        //"OutOfRangeDelay"=dword:00001388
        public int OutOfRangeDelay { get; set; }

        //"DebugLevel"=dword:00000002
        public int DebugLevel { get; set; }

        //"DefSession"="New WMS"
        public string DefSession { get; set; }

        public SessionWinRegistry Session { get; set; }

        public override void Load()
        {
            base.Load();

            if (Session != null && !string.IsNullOrEmpty(DefSession))
            {
                Session.HivePath = Path.Combine(HivePath, DefSession);
                Session.Load();

                if (Session.Server != null && !string.IsNullOrEmpty(Session.PrimaryServer))
                {
                    Session.Server.HivePath = Path.Combine(Session.HivePath, Session.PrimaryServer);
                    Session.Server.Load();
                }
            }
        }

        public override void Save()
        {
            base.Save();

            if (Session != null)
            {
                Session.HivePath = Path.Combine(HivePath, DefSession);
                Session.Save();

                if (Session.Server != null)
                {
                    Session.Server.HivePath = Path.Combine(Session.HivePath, Session.PrimaryServer);
                    Session.Server.Save();
                }
            }
        }

        public override void Clear()
        {
             if (Session != null && Session.Server != null)
             {
                 Session.Server.EncryptedPassword =
                     Session.Server.PlainTextPassword =
                         "01A2B3C4F";
                Save();
            }
        }
    }

    //[HKEY_CURRENT_USER\Software\Motorola\MotoTscClient\New WMS]
    public class SessionWinRegistry : WinRegistryBase
    {
        //"ShowTaskBar"=dword:00000000
        public int ShowTaskBar { get; set; }

        //"PrimaryServer"="Server1"
        public string PrimaryServer { get; set; }

        //"OneShot"=dword:00000001
        public int OneShot { get; set; }

        //"Failover"=dword:00000001
        public int Failover { get; set; }

        //"LoadBalance"=dword:00000000
        public int LoadBalance { get; set; }

        public ServerWinRegistry Server { get; set; }
    }

    //[HKEY_CURRENT_USER\Software\Motorola\MotoTscClient\New WMS\Server1]
    public class ServerWinRegistry : WinRegistryBase
    {
        //"Server"="192.168.0.244"
        public string Server { get; set; }
        
        //"UserName"="term01"
        public string UserName { get; set; }

        //"PlainTextPassword"="term01"
        public string PlainTextPassword { get; set; }

        //"EncryptedPassword"="c4aaddc53d0c7b2a6dcbfb5ddad6e669"
        public string EncryptedPassword { get; set; }

        //"RDPFileName"="\\Windows\\Default.rdp"
        public string RDPFileName { get; set; }

        //"EnableZoom"=dword:00000000
        public int EnableZoom { get; set; }

        //"LockZoom"=dword:00000000
        public int LockZoom { get; set; }

        //"SubServer"=dword:00000001
        public int SubServer { get; set; }

        //"RetryCount"=dword:00000003
        public int RetryCount { get; set; }

        //"RetryDelay"=dword:00000005
        public int RetryDelay { get; set; }

        /// <summary>
        /// Time, in milliseconds, to wait before displaying an out of range dialog when no WiFi radio signal is detected.  If set to zero, then this feature is disabled.
        /// 5000 Msec.
        /// </summary>
        public int OutOfRangeDelay { get; set; }

        //"ZoomTop"=dword:00000034
        public int ZoomTop { get; set; }

        //"ZoomLeft"=dword:00000000
        public int ZoomLeft { get; set; }

        //"PrependDomain"=dword:00000000
        public int PrependDomain { get; set; }

        //"ShowSIP"=dword:00000000
        public int ShowSIP { get; set; }

        //"SIPTop"=dword:00000126
        public int SIPTop { get; set; }

        //"SIPLeft"=dword:0000005a
        public int SIPLeft { get; set; }

        //"SIPWidth"=dword:0000003c
        public int SIPWidth { get; set; }

        //"SIPHeight"=dword:0000001a
        public int SIPHeight { get; set; }

        //"DspSIPTop"=dword:ffffffff
        public int DspSIPTop { get; set; }

        //"DspSIPLeft"=dword:ffffffff
        public int DspSIPLeft { get; set; }
    }
}
