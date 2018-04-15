#pragma warning disable 1587
/// --------------------------------------------------------------------------------------
/// <Copyright File="wmsEPSOTCPrint.cs" Company="ЗАО Логистическая компания МОЛКОМ">
///   Copyright (c) ЗАО Логистическая компания МОЛКОМ. All rights reserved.
/// </Copyright>
/// <Author>Vladimir G. Nosov</Author>
/// <Date>04.10.2012 8:23:18</Date>
/// <Summary>Реализация задачи печати</Summary>
/// --------------------------------------------------------------------------------------
#pragma warning restore 1587

using System;
using System.IO;
using System.Linq;
using log4net;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.EPS.wmsEPS.OutputTasks
{
    /// <summary>
    /// Реализация задачи печати
    /// </summary>
    public class EpsOtcPrint : EpsOtcBase
    {
        public override void DoTask(EpsFastReport[] reports)
        {
            if (reports == null) 
                return;

            if (Reserve)
            {
                if (FindByName(EpsTaskParams.FileFormat) == null)
                {
                    throw new ArgumentException("Expected FileFormat parameter");
                }

                ProcessFiles(reports);
            }

            var printerName = GetParamValue<string>(EpsTaskParams.PhysicalPrinter);
            if (string.IsNullOrEmpty(printerName))
                throw new ArgumentException("Expected PhysicalPrinter parameter");

            var logger = LogManager.GetLogger(GetType());
            foreach (var report in reports.Where(p => p != null))
            {
                var copies = FindByName(EpsTaskParams.Copies, Path.GetFileName(report.ReportFileName)) ??
                    FindByName(EpsTaskParams.Copies);

                report.Print(printerName, copies.Value.To<short>(0));
                logger.Info(string.Format("Report '{0}' was sent to printer '{1}' in '{2}' cop.", report.ReportFileName, printerName, copies));
            }
        }
    }
}
