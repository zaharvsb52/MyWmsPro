namespace wmsMLC.Business.Objects
{
    public class ReportFilter : WMSBusinessObject
    {
        #region . Constants .
        public const string OBJECTLOOKUPCODE_RPropertyName = "OBJECTLOOKUPCODE_R";
        public const string REPORTFILTERDATATYPEPropertyName = "REPORTFILTERDATATYPE";
        public const string REPORTFILTERDEFAULTVALUEPropertyName = "REPORTFILTERDEFAULTVALUE";
        public const string REPORTFILTERDESCPropertyName = "REPORTFILTERDESC";
        public const string REPORTFILTERDISPLAYNAMEPropertyName = "REPORTFILTERDISPLAYNAME";
        public const string REPORTFILTERFORMATPropertyName = "REPORTFILTERFORMAT";
        public const string REPORTFILTERIDPropertyName = "REPORTFILTERID";
        public const string REPORTFILTERMETHODPropertyName = "REPORTFILTERMETHOD";
        public const string REPORTFILTERORDERPropertyName = "REPORTFILTERORDER";
        public const string REPORTFILTERPARAMETERPropertyName = "REPORTFILTERPARAMETER";
        public const string REPORT_RPropertyName = "REPORT_R";
        #endregion

        public object Value { get; set; }

        #region . Properties .
        public string REPORTFILTERDESC
        {
            get { return GetProperty<string>(REPORTFILTERDESCPropertyName); }
            set { SetProperty(REPORTFILTERDESCPropertyName, value); }
        }

        public string REPORTFILTERDISPLAYNAME
        {
            get { return GetProperty<string>(REPORTFILTERDISPLAYNAMEPropertyName); }
            set { SetProperty(REPORTFILTERDISPLAYNAMEPropertyName, value); }
        }

        public string REPORTFILTERPARAMETER
        {
            get { return GetProperty<string>(REPORTFILTERPARAMETERPropertyName); }
            set { SetProperty(REPORTFILTERPARAMETERPropertyName, value); }
        }

        public string REPORTFILTERMETHOD
        {
            get { return GetProperty<string>(REPORTFILTERMETHODPropertyName); }
            set { SetProperty(REPORTFILTERMETHODPropertyName, value); }
        }

        public string REPORTFILTERDEFAULTVALUE
        {
            get { return GetProperty<string>(REPORTFILTERDEFAULTVALUEPropertyName); }
            set { SetProperty(REPORTFILTERDEFAULTVALUEPropertyName, value); }
        }

        public decimal? REPORTFILTERDATATYPE
        {
            get { return GetProperty<decimal?>(REPORTFILTERDATATYPEPropertyName); }
            set { SetProperty(REPORTFILTERDATATYPEPropertyName, value); }
        }

        public string OBJECTLOOKUPCODE_R
        {
            get { return GetProperty<string>(OBJECTLOOKUPCODE_RPropertyName); }
            set { SetProperty(OBJECTLOOKUPCODE_RPropertyName, value); }
        }

        public string REPORTFILTERFORMAT
        {
            get { return GetProperty<string>(REPORTFILTERFORMATPropertyName); }
            set { SetProperty(REPORTFILTERFORMATPropertyName, value); }
        }

        public decimal? REPORTFILTERORDER
        {
            get { return GetProperty<decimal?>(REPORTFILTERORDERPropertyName); }
            set { SetProperty(REPORTFILTERORDERPropertyName, value); }
        }
        #endregion
    }
}
