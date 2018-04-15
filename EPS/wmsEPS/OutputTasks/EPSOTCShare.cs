#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSOTCShare.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>12.10.2012 13:09:19</Date>
/// <Summary>Реализация задачи сетевого копирования файлов</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.IO;
using System.Linq;
using log4net;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    public class EpsOtcShare : EpsOtcBase
    {
        public override void DoTask(EpsFastReport[] reports)
        {
            var targetFolder = GetParamValue<string>(EpsTaskParams.TargetFolder);
            bool deleteSrcFile = FindByName(EpsTaskParams.MoveFile) != null && FindByName(EpsTaskParams.CopyFile) == null;

            var fileRecordProtect = GetParamValue<EpsTaskProtect>(EpsTaskParams.FileRecordProtect);
            var supportTargetFolder = GetParamValue<string>(EpsTaskParams.SupportTargetFolder);
            var supportFileName = GetParamValue<string>(EpsTaskParams.SupportFileName);

            var share = new EpsShare(targetFolder, deleteSrcFile);
            share.SetFileRecordProtect(fileRecordProtect, supportTargetFolder, supportFileName);

            var fileMask = GetParamValue<string>(EpsTaskParams.FileMask);
            if (!string.IsNullOrEmpty(fileMask))
            {
                FilePumper.AddSearchPattern(fileMask);
            }

            var sourceFolder = GetParamValue<string>(EpsTaskParams.SourceFolder);
            if (!string.IsNullOrEmpty(sourceFolder))
            {
                FilePumper.SetSourcePath(sourceFolder);
            }

            FilePumper.SetDestinationMove(false);
            // Вызывать ProcessFiles только после обработки параметров задачи
            ProcessFiles(reports);

            if (Files != null)
            {
                foreach (var fs in Files)
                {
                    share.Write(fs.FullFileName, fs.Data);
                }
            }

            var logger = LogManager.GetLogger(GetType());
            logger.Info(string.Format("Share: file '{0}' was shared to folder '{1}'.",
                string.Join(", ", Files.Select(p => Path.GetFileName(p.FullFileName))), targetFolder));
        }
    }
}
