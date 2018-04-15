namespace wmsMLC.Business.Objects
{
    public class AreaType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string AreaTypeCodePropertyName = "AreaTypeCode";
        #endregion

        #region .  Properties  .
        public string AreaTypeCode
        {
            get { return GetProperty<string>(AreaTypeCodePropertyName); }
            set { SetProperty(AreaTypeCodePropertyName, value); }
        }
        #endregion
    }
}
