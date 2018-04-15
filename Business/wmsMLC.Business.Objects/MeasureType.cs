namespace wmsMLC.Business.Objects
{
    public class MeasureType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MeasureTypeCodePropertyName = "MeasureTypeCode";
        public const string MeasureTypeNamePropertyName = "MeasureTypeName";
        #endregion

        #region .  Properties  .
        public string MeasureTypeCode
        {
            get { return GetProperty<string>(MeasureTypeCodePropertyName); }
            set { SetProperty(MeasureTypeCodePropertyName, value); }
        }

        public string MeasureTypeName
        {
            get { return GetProperty<string>(MeasureTypeNamePropertyName); }
            set { SetProperty(MeasureTypeNamePropertyName, value); }
        }
        #endregion
    }
}