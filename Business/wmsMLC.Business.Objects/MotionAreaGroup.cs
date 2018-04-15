using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Objects
{
    public class MotionAreaGroup : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MotionAreaGroupParentPropertyName = "MOTIONAREAGROUPPARENT";
        public const string MotionArea2GroupLPropertyName = "MOTIONAREA2GROUPL";
        #endregion .  Constants  .

        #region .  Properties  .
        public WMSBusinessCollection<MotionArea2Group> MotionArea2GroupL
        {
            get { return GetProperty<WMSBusinessCollection<MotionArea2Group>>(MotionArea2GroupLPropertyName); }
            set
            {
                SetProperty(MotionArea2GroupLPropertyName, value);
            }
        }

        [ValidateParentReference]
        public string MotionAreaGroupParent
        {
            get { return GetProperty<string>(MotionAreaGroupParentPropertyName); }
            set { SetProperty(MotionAreaGroupParentPropertyName, value); }
        }
        #endregion .  Properties  .

    }
}