namespace wmsMLC.Business.Objects
{
    public class KitType : WMSBusinessObject
    {
        #region .  Constants  .

        public const string KitTypeCodePropertyName = "KITTYPECODE";
        public const string KitTypeNamePropertyName = "KITTYPENAME";
        public const string KitTypeDescPropertyName = "KITTYPEDESC";

        #endregion

        #region .  Properties  .

        public string KitTypeCode
        {
            get { return GetProperty<string>(KitTypeCodePropertyName); }
            set { SetProperty(KitTypeCodePropertyName, value); }
        }
        public string KitTypeName
        {
            get { return GetProperty<string>(KitTypeNamePropertyName); }
            set { SetProperty(KitTypeNamePropertyName, value); }
        }
        public string KitTypeDesc
        {
            get { return GetProperty<string>(KitTypeDescPropertyName); }
            set { SetProperty(KitTypeDescPropertyName, value); }
        }

        #endregion
    }
}

