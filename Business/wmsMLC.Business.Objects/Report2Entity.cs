using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class Report2Entity : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Report2EntityReportPropertyName = "REPORT2ENTITYREPORT";
        public const string Report2EntityObjectNamePropertyName = "REPORT2ENTITYOBJECTNAME";
        #endregion

        #region .  Properties  .
        public string Report2EntityReport
        {
            get { return GetProperty<string>(Report2EntityReportPropertyName); }
            set { SetProperty(Report2EntityReportPropertyName, value); }
        }

        public string Report2EntityObjectName
        {
            get { return GetProperty<string>(Report2EntityObjectNamePropertyName); }
            set { SetProperty(Report2EntityObjectNamePropertyName, value); }
        }
        #endregion
    }
}