using System;
using System.Diagnostics;
using System.IO;
using wmsMLC.RCL.Launcher.Properties;

namespace wmsMLC.RCL.Launcher.Common
{
    public class MotoTscClientProcess : RdpProcessBase
    {
        private RdpWinRegistry _rdpreg;

        public override void Run()
        {
            if (!Validate())
                return;

            var rdcPath = CreatePath(Settings.Default.RdcPath);

            //Убиваем все, что движется
            KillProcessByFileName(Path.GetFileName(rdcPath));

            var rdpFile = CreatePath(Settings.Default.RdpFileTemplatePath);

            var retryCount = Settings.Default.RetryCount;
            if (retryCount <= 0)
                retryCount = 1;

            var debugLevel = Settings.Default.DebugLevel;
            if (debugLevel < 0)
                debugLevel = 0;

            _rdpreg = new RdpWinRegistry
            {
                //DebugLogFile = Path.Combine("Windows", "rclLauncherDebugLog.txt"),
                UnicodeTextFile = Path.Combine(Path.GetDirectoryName(rdcPath), "MotoTscClientEnglishUTF.xml"),
                OutOfRangeDelay = 5000,
                DebugLevel = debugLevel,
                DefSession = "WmsRcl",
                Session = new SessionWinRegistry
                {
                    PrimaryServer = "Server1",
                    OneShot = 1,
                    Failover = 1,
                    Server = new ServerWinRegistry
                    {
                        Server = Settings.Default.Server,
                        UserName = string.Format(@"{0}\{1}", Settings.Default.Domain, Login),
                        PlainTextPassword = Pwd,
                        EncryptedPassword = Pwd,
                        RDPFileName = rdpFile,
                        SubServer = 1,
                        RetryCount = retryCount,
                        RetryDelay = 5,
                        //OutOfRangeDelay = 5000,
                        OutOfRangeDelay = Int16.MaxValue,
                        EnableZoom = 0,
                        ZoomTop = 52,
                        SIPTop = 294,
                        SIPLeft = 90,
                        SIPWidth = 60,
                        SIPHeight = 26,
                        DspSIPTop = -1,
                        DspSIPLeft = -1
                    }
                }
            };

            _rdpreg.Save();

            //Стартуем процесс
            Start(new ProcessStartInfo
            {
                FileName = rdcPath
            });
        }

        public override void Clear()
        {
            if (_rdpreg != null)
                _rdpreg.Clear();
        }

        public override void Delete()
        {
            new RdpWinRegistry().Delete();
        }

        public override void Dispose()
        {
            base.Dispose();
            _rdpreg = null;
        }
    }
}
