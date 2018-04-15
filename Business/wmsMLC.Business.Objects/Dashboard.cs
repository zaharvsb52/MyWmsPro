namespace wmsMLC.Business.Objects
{
    public class Dashboard : WMSBusinessObject
    {
        #region .  Constants  .
        public const string DashboardBodyPropertyName = "DASHBOARDBODY";
        public const string DashboardVersionPropertyName = "DASHBOARDVERSION";

        #endregion

        #region .  Properties  .
        public string DashboardBody
        {
            get { return GetProperty<string>(DashboardBodyPropertyName); }
            set { SetProperty(DashboardBodyPropertyName, value); }
        }
        public string DashboardVersion
        {
            get { return GetProperty<string>(DashboardVersionPropertyName); }
            set { SetProperty(DashboardVersionPropertyName, value); }
        }
        #endregion
    }
}