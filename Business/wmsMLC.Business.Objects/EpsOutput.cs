namespace wmsMLC.Business.Objects
{
    public class EpsOutput : WMSBusinessObject
    {        
        #region .  Constants  .
        public const string ReportFilePropertyName = "pReportFile";
        public const string ResultReportFilePropertyName = "pResultReportFile";
        public const string FileFormatPropertyName = "pFileFormat";
        public const string ReportParam1PropertyName = "pReportParam1";
        public const string ReportParam2PropertyName = "pReportParam2";
        public const string ReportValue1PropertyName = "pReportValue1";
        public const string ReportValue2PropertyName = "pReportValue2";
        #endregion

        #region .  Properties  .
        public string ReportFile
        {
            get { return GetProperty<string>(ReportFilePropertyName); }
            set { SetProperty(ReportFilePropertyName, value); }
        }

        public string ResultReportFile
        {
            get { return GetProperty<string>(ResultReportFilePropertyName); }
            set { SetProperty(ResultReportFilePropertyName, value); }
        }

        public string FileFormat
        {
            get { return GetProperty<string>(FileFormatPropertyName); }
            set { SetProperty(FileFormatPropertyName, value); }
        }

        public string ReportParam1
        {
            get { return GetProperty<string>(ReportParam1PropertyName); }
            set { SetProperty(ReportParam1PropertyName, value); }
        }

        public string ReportParam2
        {
            get { return GetProperty<string>(ReportParam2PropertyName); }
            set { SetProperty(ReportParam2PropertyName, value); }
        }

        public string ReportValue1
        {
            get { return GetProperty<string>(ReportValue1PropertyName); }
            set { SetProperty(ReportValue1PropertyName, value); }
        }

        public string ReportValue2
        {
            get { return GetProperty<string>(ReportValue2PropertyName); }
            set { SetProperty(ReportValue2PropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
