using System;
using System.Linq;
using FastReport.Data;
using wmsMLC.Business.Objects;
using wmsMLC.EPS.wmsEPS.ExportFormat;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;

namespace wmsMLC.EPS.wmsEPS.Helpers
{
    public static class EpsHelper
    {
        public const string ReportNameMacro = "${REPORTNAME}";

        public static object GetKey(IKeyHandler data)
        {
            if (data == null) 
                throw new ArgumentNullException("data");
            return data.GetKey();
        }

        public static string GetFileExtension(string fileFormat)
        {
            using (var epsFastReportExport = new ExportFormatFactory().GetExportFormat(fileFormat))
            {
                return epsFastReportExport == null ? null : epsFastReportExport.FileExtension;
            }
        }

        public static string SetFileName(string txt, string replacestring)
        {
            if (txt.IsNullOrEmptyAfterTrim()) 
                return txt;
            if (replacestring == null) 
                replacestring = string.Empty;
            if (txt.Contains(ReportNameMacro))
            {
                return txt.Replace(ReportNameMacro, replacestring);
            }
            return txt;
        }

        public static string ConvertCollectionOutputParamToString(WMSBusinessCollection<OutputParam> collection)
        {
            if (collection == null)
                return "NULL";
            var result =
                collection.Select(
                    p =>
                    string.Format("({0}='{1}',{2}='{3}',{4}='{5}')", OutputParam.OutputParamCodePropertyName,
                                  p.OutputParamCode, OutputParam.OutputParamValuePropertyName, p.OutputParamValue,
                                  OutputParam.OutputParamSubvaluePropertyName, p.OutputParamSubvalue)).ToArray();
            return string.Join(",", result);
        }

        public static void Initialize()
        {
            FastReport.Utils.RegisteredObjects.AddConnection(typeof(OracleDataConnection));
        }

        public static string GetReportCustomParameter(string report, string parameter)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<ReportCpv>>())
            {
                var reportCpv =
                    mgr.GetFiltered(string.Format("CPV2ENTITY = 'REPORT' and CPVKEY = '{0}' and CUSTOMPARAMCODE_R = '{1}'", report, parameter))
                        .FirstOrDefault();
                return reportCpv?.CPVValue;
            }
        }
    }
}
