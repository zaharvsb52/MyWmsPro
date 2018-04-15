namespace wmsMLC.Business.Objects
{
    public class MotionArea2Group : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MotionArea2GroupMotionAreaCodePropertyName = "MOTIONAREA2GROUPMOTIONAREACODE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string MotionArea2GroupMotionAreaCode
        {
            get { return GetProperty<string>(MotionArea2GroupMotionAreaCodePropertyName); }
            set { SetProperty(MotionArea2GroupMotionAreaCodePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}