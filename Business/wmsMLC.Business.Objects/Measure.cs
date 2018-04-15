namespace wmsMLC.Business.Objects
{
    public class Measure : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MeasureShortNamePropertyName = "MEASURESHORTNAME";
        public const string MeasureCodePropertyName = "MEASURECODE";
        public const string MeasureNamePropertyName = "MEASURENAME";
        public const string MeasureTypeCodeRPropertyName = "MEASURETYPECODE_R";
        public const string MeasureFactorPropertyName = "MEASUREFACTOR";

        #endregion

        #region .  Properties  .
        public string MeasureShortName
        {
            get { return GetProperty<string>(MeasureShortNamePropertyName); }
            set { SetProperty(MeasureShortNamePropertyName, value); }
        }
        public string MeasureCode
        {
            get { return GetProperty<string>(MeasureCodePropertyName); }
            set { SetProperty(MeasureCodePropertyName, value); }
        }
        public string MeasureName
        {
            get { return GetProperty<string>(MeasureNamePropertyName); }
            set { SetProperty(MeasureNamePropertyName, value); }
        }
        public string MeasureTypeCodeR
        {
            get { return GetProperty<string>(MeasureTypeCodeRPropertyName); }
            set { SetProperty(MeasureTypeCodeRPropertyName, value); }
        }
        public decimal MeasureFactor
        {
            get { return GetProperty<decimal>(MeasureFactorPropertyName); }
            set { SetProperty(MeasureFactorPropertyName, value); }
        }
        #endregion
    }
}