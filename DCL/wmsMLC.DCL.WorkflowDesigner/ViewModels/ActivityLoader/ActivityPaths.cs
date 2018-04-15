using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.ActivityLoader
{
    public class ActivityPaths
    {
        private static string fileName;
        private static string destFileName;

        static ActivityPaths()
        {
            fileName = "activities";
            destFileName = "activityModules";

            fileName = ConfigurationManager.AppSettings.AllKeys.Contains("activityFolder") ? ConfigurationManager.AppSettings["activityFolder"] : fileName;
            destFileName = ConfigurationManager.AppSettings.AllKeys.Contains("activityFolder") ? ConfigurationManager.AppSettings["compiledActivityFolder"] : destFileName;

            if (File.Exists(Assembly.GetExecutingAssembly().Location + ".config"))
            {
                var dllConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

                AppSettingsSection appSettings = (AppSettingsSection)dllConfig.GetSection("appSettings");
                fileName = appSettings.Settings.AllKeys.Contains("activityFolder") ? appSettings.Settings["activityFolder"].Value : fileName;
                destFileName = appSettings.Settings.AllKeys.Contains("activityFolder") ? appSettings.Settings["compiledActivityFolder"].Value : destFileName;
            }

            fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            destFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, destFileName);

            if (!Directory.Exists(fileName)) Directory.CreateDirectory(fileName);
            if (!Directory.Exists(destFileName)) Directory.CreateDirectory(destFileName);
        }

        public static string DestFileName
        {
            get
            {
                return destFileName;
            }
        }

        public static string FileName
        {
            get
            {
                return fileName;
            }
        }
    }
}
