namespace wmsMLC.Business.Objects
{
    public class TE2Blocking : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TE2BlockingIdPropertyName = "TE2BlockingId";
        public const string TECodePropertyName = "TE2BlockingTECode";
        public const string BlockingCodePropertyName = "TE2BlockingBlockingCode";
        public const string TE2BlockingDescPropertyName = "TE2BlockingDesc";
        #endregion

        #region .  Properties  .
        public decimal? TE2BlockingId
        {
            get { return GetProperty<decimal?>(TE2BlockingIdPropertyName); }
            set { SetProperty(TE2BlockingIdPropertyName, value); }
        }

        public string TE2BlockingTECode
        {
            get { return GetProperty<string>(TECodePropertyName); }
            set { SetProperty(TECodePropertyName, value); }
        }

        public string TE2BlockingBlockingCode
        {
            get { return GetProperty<string>(BlockingCodePropertyName); }
            set { SetProperty(BlockingCodePropertyName, value); }
        }

        public string TE2BlockingDesc
        {
            get { return GetProperty<string>(TE2BlockingDescPropertyName); }
            set { SetProperty(TE2BlockingDescPropertyName, value); }
        }
        #endregion
    }
}
