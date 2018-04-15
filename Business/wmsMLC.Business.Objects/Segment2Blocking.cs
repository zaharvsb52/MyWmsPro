namespace wmsMLC.Business.Objects
{
    public class Segment2Blocking : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Segment2BlockingIdPropertyName = "Segment2BlockingId";
        public const string SegmentCodePropertyName = "Segment2BlockingSegmentCode";
        public const string BlockingCodePropertyName = "Segment2BlockingBlockingCode";
        public const string Segment2BlockingDescPropertyName = "Segment2BlockingDesc";
        #endregion

        #region .  Properties  .
        public decimal? Segment2BlockingId
        {
            get { return GetProperty<decimal?>(Segment2BlockingIdPropertyName); }
            set { SetProperty(Segment2BlockingIdPropertyName, value); }
        }

        public string Segment2BlockingSegmentCode
        {
            get { return GetProperty<string>(SegmentCodePropertyName); }
            set { SetProperty(SegmentCodePropertyName, value); }
        }

        public string Segment2BlockingBlockingCode
        {
            get { return GetProperty<string>(BlockingCodePropertyName); }
            set { SetProperty(BlockingCodePropertyName, value); }
        }

        public string Segment2BlockingDesc
        {
            get { return GetProperty<string>(Segment2BlockingDescPropertyName); }
            set { SetProperty(Segment2BlockingDescPropertyName, value); }
        }
        #endregion
    }
}
