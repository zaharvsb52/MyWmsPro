namespace wmsMLC.Business.Objects
{
    public class Output : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Login_RPropertyName = "LOGIN_R";
        public const string Host_RPropertyName = "HOST_R";
        public const string ReportFileSubfolder_RPropertyName = "REPORTFILESUBFOLDER_R";
        //public const string ReportFilePropertyName = "REPORTFILE_R";
        //public const string ReportPropertyName = "REPORT_R";
        public const string OutputStatusPropertyName = "OUTPUTSTATUS";
        public const string OutputTimePropertyName = "OUTPUTTIME";
        public const string OutputFeedbackPropertyName = "OUTPUTFEEDBACK";
        public const string EpsHandlerPropertyName = "EPSHANDLER";
        public const string ReportParamsPropertyName = "REPORTPARAMS";
        public const string EpsParamsPropertyName = "EPSPARAMS";
        public const string OutputTasksPropertyName = "OUTPUTTASKS";
        #endregion .  Constants  .

        #region .  Properties  .
        public string Login_R
        {
            get { return GetProperty<string>(Login_RPropertyName); }
            set { SetProperty(Login_RPropertyName, value); }
        }

        public string Host_R
        {
            get { return GetProperty<string>(Host_RPropertyName); }
            set { SetProperty(Host_RPropertyName, value); }
        }

        public string ReportFileSubfolder_R
        {
            get { return GetProperty<string>(ReportFileSubfolder_RPropertyName); }
            set { SetProperty(ReportFileSubfolder_RPropertyName, value); }
        }

        public string OutputStatus
        {
            get { return GetProperty<string>(OutputStatusPropertyName); }
            set { SetProperty(OutputStatusPropertyName, value); }
        }

        public string OutputTime
        {
            get { return GetProperty<string>(OutputTimePropertyName); }
            set { SetProperty(OutputTimePropertyName, value); }
        }

        public string OutputFeedback
        {
            get { return GetProperty<string>(OutputFeedbackPropertyName); }
            set { SetProperty(OutputFeedbackPropertyName, value); }
        }

        public decimal EpsHandler
        {
            get { return GetProperty<decimal>(EpsHandlerPropertyName); }
            set { SetProperty(EpsHandlerPropertyName, value); }
        }

        public WMSBusinessCollection<OutputParam> ReportParams
        {
            get { return GetProperty<WMSBusinessCollection<OutputParam>>(ReportParamsPropertyName); }
            set { SetProperty(ReportParamsPropertyName, value); }
        }

        public WMSBusinessCollection<OutputParam> EpsParams
        {
            get { return GetProperty<WMSBusinessCollection<OutputParam>>(EpsParamsPropertyName); }
            set { SetProperty(EpsParamsPropertyName, value); }
        }

        public WMSBusinessCollection<OutputTask> OutputTasks
        {
            get { return GetProperty<WMSBusinessCollection<OutputTask>>(OutputTasksPropertyName); }
            set { SetProperty(OutputTasksPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}