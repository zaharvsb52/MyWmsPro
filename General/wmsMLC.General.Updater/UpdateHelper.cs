using System;
using System.IO;
using System.Xml;


namespace wmsMLC.General.Updater
{
    public static class UpdateHelper
    {
        public static UpdateInfo ReadUpdateInfo(string currentPath, string updateInfoFileName, out bool isCantCopyToLocal)
        {
            isCantCopyToLocal = false;
            var flUpdateInfo = new FileInfo(updateInfoFileName);

            // если файла нет, то считаем что идет обновление системы
            if (!flUpdateInfo.Exists)
            {
                return new UpdateInfo {Updating = true};
            }

            string localFlUpdateInfo;
            try
            {
                localFlUpdateInfo = Path.Combine(currentPath, flUpdateInfo.Name);                
                flUpdateInfo.CopyTo(localFlUpdateInfo, true);
            }
            catch
            {
                isCantCopyToLocal = true;
                try
                {
                    localFlUpdateInfo = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), flUpdateInfo.Name);
                    flUpdateInfo.CopyTo(localFlUpdateInfo, true);
                }
                catch 
                {
                    throw new Exception("Не возможно проверить версию. Не удается скопировать файл с версией ни в папку temp ни в локальную папку");
                }
            }
                
            var updInfo = new UpdateInfo();

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(localFlUpdateInfo);
            updInfo.LastVersion = xmlDocument.GetElementsByTagName("LastVersion")[0].InnerText;
            updInfo.MinimalSupportVersion = xmlDocument.GetElementsByTagName("MinimalSupportVersion")[0].InnerText;

            updInfo.LogPath = xmlDocument.GetElementsByTagName("LogPath")[0].InnerText;
            updInfo.ProcessName = xmlDocument.GetElementsByTagName("ProcessName")[0].InnerText;
            updInfo.Text = xmlDocument.GetElementsByTagName("Text")[0].InnerText;
            updInfo.UpdateTool = xmlDocument.GetElementsByTagName("UpdateTool")[0].InnerText;
            updInfo.DistributivePath = xmlDocument.GetElementsByTagName("DistributivePath")[0].InnerText;

            try
            {
                File.Delete(localFlUpdateInfo);
            }
            catch 
            {

            }

            return updInfo;
        }

        public static bool CheckForUpdate(string currentVersion, string lastVersion, string minimalSupportVersion, out bool isCritical)
        {
            isCritical = false;

            var currentVer = new Version(currentVersion);
            var lastVer = new Version(lastVersion);

            if (currentVer == lastVer)
                return false;

            if (currentVer > lastVer)
                throw new Exception("Текущая версия не может быть больше серверной");

            var minSupportVer = new Version(minimalSupportVersion);
            if (currentVer < minSupportVer)
            {
                isCritical = true;
                return true;
            }

            if (currentVer < lastVer)
                return true;

            throw new Exception("Неожиданное поведение. Проверка версии не удалась");
        }

        public static string GetUpdater(string currentPath, string destinationUpdaterFileName)
        {
            var destinationUpdaterTool = new FileInfo(destinationUpdaterFileName);

            var currentUpdaterToolFileName = Path.Combine(currentPath, destinationUpdaterTool.Name);
            var currentUpdaterToolFileInfo = new FileInfo(currentUpdaterToolFileName);

            if (!currentUpdaterToolFileInfo.Exists || currentUpdaterToolFileInfo.LastWriteTime < destinationUpdaterTool.LastWriteTime)
                currentUpdaterToolFileInfo = destinationUpdaterTool.CopyTo(currentUpdaterToolFileName, true);

            return currentUpdaterToolFileInfo.FullName;
        }

        public static void RunUpdater(string fileUpdater, string updateInfoFileName)
        {
            System.Diagnostics.Process.Start(fileUpdater, updateInfoFileName);
        }
    }
}
