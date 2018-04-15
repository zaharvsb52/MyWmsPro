using System;
using System.Linq;
using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class Report : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ReportLockedPropertyName = "REPORTLOCKED";
        public const string ReportCopiesPropertyName = "REPORTCOPIES";
        public const string ReportFile_RPropertyName = "REPORTFILE_R";
        public const string ReportNamePropertyName = "REPORTNAME";
        public const string EpsHandlerPropertyName = "EPSHANDLER";
        public const string ConfigRepPropertyName = "CONFIGREP";
        public const string ChildReportsPropertyName = "CHILD2REPORTL";
        public const string ParentsReportsPropertyName = "PARENT2REPORTL";
        public const string ReportFilterLPropertyName = "REPORTFILTERL";
        #endregion .  Constants  .

        #region .  Properties  .

        public bool ReportLocked
        {
            get { return GetProperty<bool>(ReportLockedPropertyName); }
            set { SetProperty(ReportLockedPropertyName, value); }
        }

        public decimal ReportCopies
        {
            get { return GetProperty<decimal>(ReportCopiesPropertyName); }
            set { SetProperty(ReportCopiesPropertyName, value); }
        }

        public string ReportName
        {
            get { return GetProperty<string>(ReportNamePropertyName); }
            set { SetProperty(ReportNamePropertyName, value); }
        }

        public string ReportFile_R
        {
            get { return GetProperty<string>(ReportFile_RPropertyName); }
            set { SetProperty(ReportFile_RPropertyName, value); }
        }

        public decimal EpsHandler
        {
            get { return GetProperty<decimal>(EpsHandlerPropertyName); }
            set { SetProperty(EpsHandlerPropertyName, value); }
        }

        public WMSBusinessCollection<ReportCfg> ConfigRep
        {
            get { return GetProperty<WMSBusinessCollection<ReportCfg>>(ConfigRepPropertyName); }
            set { SetProperty(ConfigRepPropertyName, value); }
        }

        public WMSBusinessCollection<Report2Report> ChildReports
        {
            get { return GetProperty<WMSBusinessCollection<Report2Report>>(ChildReportsPropertyName); }
            set { SetProperty(ChildReportsPropertyName, value); }
        }

        public WMSBusinessCollection<Report2Report> ParentsReports
        {
            get { return GetProperty<WMSBusinessCollection<Report2Report>>(ParentsReportsPropertyName); }
            set { SetProperty(ParentsReportsPropertyName, value); }
        }

        public WMSBusinessCollection<ReportFilter> ReportFilterL
        {
            get { return GetProperty<WMSBusinessCollection<ReportFilter>>(ReportFilterLPropertyName); }
            set { SetProperty(ReportFilterLPropertyName, value); }
        }

        public bool IsChildReports
        {
            get { return ChildReports != null && ChildReports.Count > 0; }
        }
        #endregion .  Properties  .

        public bool IsBatchPrint
        {
            get
            {
                return ConfigRep != null
                    && ConfigRep.Any(c =>
                        c != null
                        && EpsTaskParams.BatchPrint.ToString("G").Equals(c.EpsConfigParamCode, StringComparison.OrdinalIgnoreCase)
                        && (c.EpsConfigValue.To(0) != 0));
            }
        }
    }
}
