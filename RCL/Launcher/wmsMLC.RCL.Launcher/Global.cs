using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace wmsMLC.RCL.Launcher
{
    public static class Global
    {
        public static string AppName
        {
            get
            {
                var path = Process.GetCurrentProcess().StartInfo.FileName;
                return Path.GetFileNameWithoutExtension(path);
            }
        }

        public static string AppPath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase); }
        }

        public static string AppShortName
        {
            get { return "RCL Launcher"; }
        }

        public static string HivePath
        {
            get { return Path.Combine(@"Software\My", AppName); }
        }

        public static string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public static string AppNamePlusVersion
        {
            get { return string.Format("{0} {1}", AppShortName, Version); }
        }

        public static void ShowMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            MessageBox.Show(message, AppShortName,
                MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
        }

        public static void ShowError(Exception ex)
        {
            if (ex == null)
                return;
            ShowError(ex.Message);
        }

        public static void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            MessageBox.Show(string.Format("{0} {1}", Properties.Resources.Error, message), AppShortName,
                MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1);
        }
    }

    public enum RdcType
    {
        MotoTscClient,
        CetscHoneywell
    }
}
