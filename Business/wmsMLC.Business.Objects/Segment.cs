namespace wmsMLC.Business.Objects
{
    public class Segment : WMSBusinessObject
    {
        #region .  Constants  .
        public const string SegmentCodePropertyName = "SEGMENTCODE";
        public const string AreaCode_RPropertyName = "AREACODE_R";
        public const string SegmentTypeCode_RPropertyName = "SEGMENTTYPECODE_R";
        public const string SegmentNumberPropertyName = "SEGMENTNUMBER";
        public const string SegmentPosXPropertyName = "SEGMENTPOSX";
        public const string SegmentPosYPropertyName = "SEGMENTPOSY";
        public const string SegmentAnglePropertyName = "SEGMENTANGLE";
        
        #endregion .  Constants  .

        #region .  Properties  .
        public string SegmentCode
        {
            get { return GetProperty<string>(GetPrimaryKeyPropertyName()); }
            set { SetProperty(GetPrimaryKeyPropertyName(), value); }
        }

        public string AreaCode_R
        {
            get { return GetProperty<string>(AreaCode_RPropertyName); }
            set { SetProperty(AreaCode_RPropertyName, value); }
        }

        public string SegmentTypeCode_R
        {
            get { return GetProperty<string>(SegmentTypeCode_RPropertyName); }
            set { SetProperty(SegmentTypeCode_RPropertyName, value); }
        }

        public string SegmentNumber
        {
            get { return GetProperty<string>(SegmentNumberPropertyName); }
            set { SetProperty(SegmentNumberPropertyName, value); }
        }

        public decimal SegmentPosX
        {
            get { return GetProperty<decimal>(SegmentPosXPropertyName); }
            set { SetProperty(SegmentPosXPropertyName, value); }
        }

        public decimal SegmentPosY
        {
            get { return GetProperty<decimal>(SegmentPosYPropertyName); }
            set { SetProperty(SegmentPosYPropertyName, value); }
        }

        public decimal SegmentAngle
        {
            get { return GetProperty<decimal>(SegmentAnglePropertyName); }
            set { SetProperty(SegmentAnglePropertyName, value); }
        }
        #endregion
    }
}
