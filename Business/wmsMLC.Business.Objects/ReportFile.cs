namespace wmsMLC.Business.Objects
{
    public class ReportFile : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ReportFileSubfolderPropertyName = "REPORTFILESUBFOLDER";
        public const string ReportFileFilePropertyName = "REPORTFILEFILE";
        public const string ReportFileHashCodePropertyName = "REPORTFILEHASHCODE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string ReportFileSubfolder
        {
            get { return GetProperty<string>(ReportFileSubfolderPropertyName); }
            set { SetProperty(ReportFileSubfolderPropertyName, value); }
        }

        public string ReportFileFile
        {
            get { return GetProperty<string>(ReportFileFilePropertyName); }
            set { SetProperty(ReportFileFilePropertyName, value); }
        }

        public string ReportFileHashCode
        {
            get { return GetProperty<string>(ReportFileHashCodePropertyName); }
            set { SetProperty(ReportFileHashCodePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}