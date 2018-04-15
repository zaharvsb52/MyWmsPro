namespace wmsMLC.Business.Objects
{
    public class SegmentType : WMSBusinessObject
    {
        #region .  Constants  .
        public const string GlobalParamValPropertyName = "GLOBALPARAMVAL";
        public const string SegmentTypeCodeFormatPropertyName = "SEGMENTTYPECODEFORMAT";
        public const string SegmentTypeCodeViewPropertyName = "SEGMENTTYPECODEVIEW";
        #endregion .  Constants  .

        #region .  Properties  .
        public WMSBusinessCollection<SegmentTypeGPV> GlobalParamVal
        {
            get { return GetProperty<WMSBusinessCollection<SegmentTypeGPV>>(GlobalParamValPropertyName); }
            set { SetProperty(GlobalParamValPropertyName, value); }
        }
        public string SegmentTypeCodeFormat
        {
            get { return GetProperty<string>(SegmentTypeCodeFormatPropertyName); }
            set { SetProperty(SegmentTypeCodeFormatPropertyName, value); }
        }

        public string SegmentTypeCodeView
        {
            get { return GetProperty<string>(SegmentTypeCodeViewPropertyName); }
            set { SetProperty(SegmentTypeCodeViewPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}
