#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEpsCheckOut.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Olga Marysheva</Author>
/// <Date>04.10.2012 13:29:04</Date>
/// <Summary>Получение отчетов из базы</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.EPS.wmsEPS
{
    /// <summary>
    /// Класс для получения отчетов из базу данных и записи их.
    /// </summary>
    public class EpsCheckOut
    {
        private IBaseManager<ReportFile> GetManager()
        {
            return IoC.Instance.Resolve<IBaseManager<ReportFile>>();
        }

        /// <summary>
        /// Получение данных из базы и проверка их с файлами
        /// </summary>
        public void CheckOut()
        {
            var manager = GetManager();
            var tlist = manager.GetAll();
            if (tlist == null)
                return;

            foreach (var obj in tlist)
                CompareWithLocalReport(obj);
        }

        /// <summary>
        /// Формирвоание полного имени файла
        /// Проверка наличие поддиректории(subfolder)
        /// </summary>
        /// <param name="obj">данные из базы</param>
        /// <returns>полное имя файла</returns>
        private string GetFileName(ReportFile obj)
        {
            return obj.ReportFileSubfolder == null
                ? string.Format(@"{0}\{1}", Properties.Settings.Default.ReportPath, obj.ReportFileFile)
                : string.Format(@"{0}\{1}\{2}", Properties.Settings.Default.ReportPath, obj.ReportFileSubfolder, obj.ReportFileFile);
        }

        /// <summary>
        /// Сравнение отчетов
        /// </summary>
        /// <param name="obj">данные из базы</param>
        private void CompareWithLocalReport(ReportFile obj)
        {
            string fullFileName = GetFileName(obj);
            if (!File.Exists(fullFileName))
            {
                SaveReportLocally(fullFileName);
                return;
            }

            var hc = new HashCode();
            if (!(obj.ReportFileHashCode.Equals(hc.GetSHA1(fullFileName))))
            {
                SaveReportLocally(fullFileName);
            }
        }

        /// <summary>
        /// Запись отчета на диск.
        /// </summary>
        /// <param name="fullFileName">полное имя файла</param>
        private void SaveReportLocally(string fullFileName)
        {
            var fileName = Path.GetFileName(fullFileName);
            var manager = GetManager() as IReportFileManager;
            if (manager == null) throw new DeveloperException("Can't get IReportFileManager.");

            var blob = manager.GetReportFileBody(fileName);
            if (blob == null || blob.Length == 0)
            {
                throw new Exception(string.Format("Report {0} blob is empty", fileName));
            }
            else
            {
                using (var memmorystream = new MemoryStream())
                {
                    // Создаем директорию, если ее нет
                    string dirName = Path.GetDirectoryName(fullFileName);
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    using (var outStream = File.Create(fullFileName))
                    {
                        memmorystream.WriteTo(outStream);
                        outStream.Flush();
                        outStream.Close();
                    }
                }
            }
        }
    }
}
