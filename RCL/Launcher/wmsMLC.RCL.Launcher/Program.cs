using System;
using System.Windows.Forms;
using wmsMLC.RCL.Launcher.Forms;
using wmsMLC.RCL.Launcher.Properties;

namespace wmsMLC.RCL.Launcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(params string[] args)
        {
            //Параметры
            //0 - Server

            string server = null;
            if (args.Length > 0)
            {
                server = args[0];
                if (server == "?" || server == "-?" || server == "/?")
                {
                    Global.ShowMessage(string.Format(Resources.AppParametersInfo, Global.AppName, Environment.NewLine));
                    Application.Exit();
                    return;
                }
            }

            try
            {
                Settings.Default.Load();
            }
            catch (Exception ex)
            {
                Global.ShowError(string.Format("{0}{1}{2}", ex.Message, Environment.NewLine,
                    Resources.FatalErrorExit));
                Application.Exit();
                return;
            }

            if (!string.IsNullOrEmpty(server))
                Settings.Default.Server = server;

            Application.Run(new MainForm());
        }
    }
}