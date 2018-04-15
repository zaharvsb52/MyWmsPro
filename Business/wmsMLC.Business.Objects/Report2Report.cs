using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Objects
{
    public class Report2Report : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ReportCodePropertyName = "REPORT2REPORTREPORT";
        public const string PriorityPropertyName = "R2RPRIORITY";
        public const string R2RparentPropertyName = "R2RPARENT";
        #endregion .  Constants  .

        #region .  Properties  .

        public string ReportCode
        {
            get { return GetProperty<string>(ReportCodePropertyName); }
            set { SetProperty(ReportCodePropertyName, value); }
        }

        public decimal Priority
        {
            get { return GetProperty<decimal>(PriorityPropertyName); }
            set { SetProperty(PriorityPropertyName, value); }
        }

        [ValidateParentReference(ReportCodePropertyName)]
        public string R2Rparent
        {
            get { return GetProperty<string>(R2RparentPropertyName); }
            set { SetProperty(R2RparentPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}