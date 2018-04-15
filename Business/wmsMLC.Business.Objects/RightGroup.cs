using wmsMLC.General.BL.Validation.Attributes;

namespace wmsMLC.Business.Objects
{
    public class RightGroup : WMSBusinessObject
    {
        #region .  Constants  .
        public const string RightGroupParentPropertyName = "RIGHTGROUPPARENT";
        #endregion

        #region .  Properties  .
        [ValidateParentReference]
        public string RightGroupParent
        {
            get { return GetProperty<string>(RightGroupParentPropertyName); }
            set { SetProperty(RightGroupParentPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}

