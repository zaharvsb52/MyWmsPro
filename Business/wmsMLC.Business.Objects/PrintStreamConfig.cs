using System;

namespace wmsMLC.Business.Objects
{
    public class PrintStreamConfig : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Host_RPropertyName = "HOST_R";
        public const string Login_RPropertyName = "LOGIN_R";
        public const string MandantID_RPropertyName = "MANDANTID";
        public const string Report_RPropertyName = "REPORT_R";
        public const string PeriodBeginPropertyName = "PERIODBEGIN";
        public const string PeriodEndPropertyName = "PERIODEND";
        public const string LogicalPrinter_RPropertyName = "LOGICALPRINTER_R";
        public const string PrintStreamCopiesPropertyName = "PRINTSTREAMCOPIES";
        public const string PrintStreamLockedPropertyName = "PRINTSTREAMLOCKED";
        public const string EventKindCode_RPropertyName = "EVENTKINDCODE_R";
        public const string FactoryID_RPropertyName = "FACTORYID_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public string Host_R
        {
            get { return GetProperty<string>(Host_RPropertyName); }
            set { SetProperty(Host_RPropertyName, value); }
        }

        public string Login_R
        {
            get { return GetProperty<string>(Login_RPropertyName); }
            set { SetProperty(Login_RPropertyName, value); }
        }

        /// <summary>
        /// Код манданта (не партнера).
        /// </summary>
        public decimal? MandantID
        {
            get { return GetProperty<decimal?>(MandantID_RPropertyName); }
            set { SetProperty(MandantID_RPropertyName, value); }
        }

        public string Report_R
        {
            get { return GetProperty<string>(Report_RPropertyName); }
            set { SetProperty(Report_RPropertyName, value); }
        }

        public DateTime? PeriodBegin
        {
            get { return GetProperty<DateTime?>(PeriodBeginPropertyName); }
            set { SetProperty(PeriodBeginPropertyName, value); }
        }

        public DateTime? PeriodEnd
        {
            get { return GetProperty<DateTime?>(PeriodEndPropertyName); }
            set { SetProperty(PeriodEndPropertyName, value); }
        }

        public string LogicalPrinter_R
        {
            get { return GetProperty<string>(LogicalPrinter_RPropertyName); }
            set { SetProperty(LogicalPrinter_RPropertyName, value); }
        }

        public decimal PrintStreamCopies
        {
            get { return GetProperty<decimal>(PrintStreamCopiesPropertyName); }
            set { SetProperty(PrintStreamCopiesPropertyName, value); }
        }

        public bool PrintStreamLocked
        {
            get { return GetProperty<bool>(PrintStreamLockedPropertyName); }
            set { SetProperty(PrintStreamLockedPropertyName, value); }
        }

        public string EventKindCode_R
        {
            get { return GetProperty<string>(EventKindCode_RPropertyName); }
            set { SetProperty(EventKindCode_RPropertyName, value); }
        }

        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FactoryID_RPropertyName); }
            set { SetProperty(FactoryID_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
