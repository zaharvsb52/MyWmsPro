namespace wmsMLC.Business.Objects
{
    public class ReportRedefinition : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Report_RPropertyName = "REPORT_R";
        public const string Host_RPropertyName = "HOST_R";
        public const string PartnerId_RPropertyName = "PARTNERID_R";
        public const string ReportRedefinitionReportPropertyName = "REPORTREDEFINITIONREPORT";
        public const string ReportRedefinitionCopiesPropertyName = "REPORTREDEFINITIONCOPIES";
        public const string ReportRedefinitionLockedPropertyName = "REPORTREDEFINITIONLOCKED";
        #endregion .  Constants  .

        #region .  Properties  .
        public string Report_R
        {
            get { return GetProperty<string>(Report_RPropertyName); }
            set { SetProperty(Report_RPropertyName, value); }
        }

        public string Host_R
        {
            get { return GetProperty<string>(Host_RPropertyName); }
            set { SetProperty(Host_RPropertyName, value); }
        }

        /// <summary>
        /// Код манданта (не партнера).
        /// </summary>
        public decimal? PartnerId_R
        {
            get { return GetProperty<decimal?>(PartnerId_RPropertyName); }
            set { SetProperty(PartnerId_RPropertyName, value); }
        }

        public string ReportRedefinitionReport
        {
            get { return GetProperty<string>(ReportRedefinitionReportPropertyName); }
            set { SetProperty(ReportRedefinitionReportPropertyName, value); }
        }

        public decimal ReportRedefinitionCopies
        {
            get { return GetProperty<decimal>(ReportRedefinitionCopiesPropertyName); }
            set { SetProperty(ReportRedefinitionCopiesPropertyName, value); }
        }

        public bool ReportRedefinitionLocked
        {
            get { return GetProperty<bool>(ReportRedefinitionLockedPropertyName); }
            set { SetProperty(ReportRedefinitionLockedPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
