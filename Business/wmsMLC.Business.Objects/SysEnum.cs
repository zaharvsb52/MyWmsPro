namespace wmsMLC.Business.Objects
{
    public class SysEnum : WMSBusinessObject
    {
        #region .  Constants  .

        public const string ENUMGROUPPropertyName = "ENUMGROUP";
        public const string ENUMKEYPropertyName = "ENUMKEY";
        public const string SysEnumNamePropertyName = "ENUMNAME";
        public const string SysEnumValuePropertyName = "ENUMVALUE";

        #endregion .  Constants  .

        #region .  Properties  .
        public string SysEnumName
        {
            get { return GetProperty<string>(SysEnumNamePropertyName); }
            set { SetProperty(SysEnumNamePropertyName, value); }
        }

        public string SysEnumValue
        {
            get { return GetProperty<string>(SysEnumValuePropertyName); }
            set { SetProperty(SysEnumValuePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}