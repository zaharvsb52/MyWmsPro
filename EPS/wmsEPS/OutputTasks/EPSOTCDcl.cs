#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSOTCDcl.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>12.10.2012 13:30:50</Date>
/// <Summary>Реализация задачи работы с клиентом</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;
using System.Linq;
using log4net;
using wmsMLC.Business.Objects;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    public class EpsOtcDcl : EpsOtcBase
    {
        /// <summary>
        /// Выполнение задачи
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void DoTask(EpsFastReport[] reports)
        {
            bool deleteSrcFile = FindByName(EpsTaskParams.MoveFile) != null && FindByName(EpsTaskParams.CopyFile) == null;

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

            var logger = LogManager.GetLogger(GetType());
            logger.Info(string.Format("Dcl: file '{0}' was dcl'ed.",
                string.Join(", ", Files.Select(p => Path.GetFileName(p.FullFileName)))));
        }
    }
}
