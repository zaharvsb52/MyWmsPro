#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSOTCFtp.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>12.10.2012 13:24:34</Date>
/// <Summary>Реализация задачи работы с FTP</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System.IO;
using System.Linq;
using log4net;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.Helpers;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    public class EpsOtcFtp : EpsOtcBase
    {
        public override void DoTask(EpsFastReport[] reports)
        {
            var targetFolder = GetParamValue<string>(EpsTaskParams.TargetFolder);
            var deleteSrcFile = FindByName(EpsTaskParams.MoveFile) != null && FindByName(EpsTaskParams.CopyFile) == null;

            var fileRecordProtect = GetParamValue<EpsTaskProtect>(EpsTaskParams.FileRecordProtect);
            var supportTargetFolder = GetParamValue<string>(EpsTaskParams.SupportTargetFolder);
            var supportFileName = GetParamValue<string>(EpsTaskParams.SupportFileName);
            var serverName = GetParamValue<string>(EpsTaskParams.FTPServerName);
            var serverLogin = GetParamValue<string>(EpsTaskParams.FTPServerLogin);
            var serverPasswd = GetParamValue<string>(EpsTaskParams.FTPServerPassword);
            var usePassive = GetParamValue<byte>(EpsTaskParams.FTPTransmissionMode) == 0;

            var ftp = new EpsFtp(targetFolder, serverName, serverLogin, serverPasswd);
            ftp.SetFileRecordProtect(fileRecordProtect, supportTargetFolder, supportFileName);
            ftp.UsePassive = usePassive;

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

            FilePumper.SetDestinationMove(deleteSrcFile);

            // Вызывать ProcessFiles только после обработки параметров задачи
            ProcessFiles(reports);

            if (Files != null)
            {
                foreach (var fs in Files)
                {
                    ftp.Write(fs.FullFileName, fs.Data);
                    if (deleteSrcFile)
                        ftp.DeleteFile(fs.FullFileName);
                }
            }

            var logger = LogManager.GetLogger(GetType());
            logger.Info(string.Format("File '{0}' was published on FTP '{1}' in folder '{2}'.",
                string.Join(", ", Files.Select(p => Path.GetFileName(p.FullFileName))), serverName, targetFolder));
        }
    }
}