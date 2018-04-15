using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using wmsMLC.RCL.Launcher.Properties;
using wmsMLC.RCL.Launcher.WinAPI;

namespace wmsMLC.RCL.Launcher.Common
{
    public abstract class RdpProcessBase : IDisposable
    {
        public string Login { get; set; }
        public string Pwd { get; set; }

        public abstract void Run();
        public abstract void Clear();
        public abstract void Delete();

        protected string CreatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            var filePath = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(filePath) ? Path.Combine(Global.AppPath, path) : path;
        }

        protected virtual void KillProcessByFileName(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            var fn = filename.ToLower();
            var list = ProcessCE.GetProcesses();
            var processes = list.Where(p => p.FullPath.ToLower().EndsWith(fn)).ToList();
            foreach (var p in processes)
            {
                p.Kill();
            }
        }

        public virtual void Start(ProcessStartInfo startInfo)
        {
            if (startInfo == null)
                throw new ArgumentNullException("startInfo");

            var pr = new Process
            {
                StartInfo = startInfo
            };
            pr.Start();
            //pr.WaitForExit();
        }

        public virtual bool Validate()
        {
            if (string.IsNullOrEmpty(Settings.Default.Server))
                throw new ApplicationException(string.Format(Resources.SettingsParameterIsNull, "Server", Settings.Default.SettingsPath));

            if (string.IsNullOrEmpty(Settings.Default.Domain))
                throw new ApplicationException(string.Format(Resources.SettingsParameterIsNull, "Domain", Settings.Default.SettingsPath));

            var rdcClientPath = Settings.Default.RdcPath;
            if (string.IsNullOrEmpty(rdcClientPath))
                throw new ApplicationException(string.Format(Resources.SettingsParameterIsNull, "RdcPath", Settings.Default.SettingsPath));

            rdcClientPath = CreatePath(rdcClientPath);
            if (!File.Exists(rdcClientPath))
                throw new FileNotFoundException(string.Format(Resources.FileNotFound, rdcClientPath));

            var rdpFileTemplatePath = Settings.Default.RdpFileTemplatePath;
            if (string.IsNullOrEmpty(rdpFileTemplatePath))
                throw new ApplicationException(string.Format(Resources.SettingsParameterIsNull, "RdpFileTemplatePath", Settings.Default.SettingsPath));

            rdpFileTemplatePath = CreatePath(rdpFileTemplatePath);
            if (!File.Exists(rdpFileTemplatePath))
                throw new FileNotFoundException(string.Format(Resources.FileNotFound, rdpFileTemplatePath));

            return true;
        }

        #region IDisposable Members

        public virtual void Dispose()
        {

        }

        #endregion IDisposable Members
    }
}
