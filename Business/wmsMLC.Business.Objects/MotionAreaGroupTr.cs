using wmsMLC.General;

namespace wmsMLC.Business.Objects
{

    [SourceName("MotionAreaGroupTree")]
    public class MotionAreaGroupTr : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MotionAreaGroupTreeParentPropertyName = "MOTIONAREAGROUPCODEPARENT";
        public const string MotionAreaGroupTreePropertyName = "MOTIONAREAGROUPCODE_R";
        #endregion

        #region .  Properties  .
        public string MotionAreaGroupCodeParent
        {
            get { return GetProperty<string>(MotionAreaGroupTreeParentPropertyName); }
            set { SetProperty(MotionAreaGroupTreeParentPropertyName, value); }
        }
        public string MotionAreaGroupCode
        {
            get { return GetProperty<string>(MotionAreaGroupTreePropertyName); }
            set { SetProperty(MotionAreaGroupTreePropertyName, value); }
        }
        #endregion
    }
}