namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Сущность 'Области к блокировкам'.
    /// </summary>
    public class Area2Blocking : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Area2BlockingIdPropertyName = "Area2BlockingId";
        public const string AreaCodePropertyName = "Area2BlockingAreaCode";
        public const string BlockingCodePropertyName = "Area2BlockingBlockingCode";
        public const string Area2BlockingDescPropertyName = "Area2BlockingDesc";
        #endregion

        #region .  Properties  .
        public decimal? Area2BlockingId
        {
            get { return GetProperty<decimal?>(Area2BlockingIdPropertyName); }
            set { SetProperty(Area2BlockingIdPropertyName, value); }
        }

        public string Area2BlockingAreaCode
        {
            get { return GetProperty<string>(AreaCodePropertyName); }
            set { SetProperty(AreaCodePropertyName, value); }
        }

        public string Area2BlockingBlockingCode
        {
            get { return GetProperty<string>(BlockingCodePropertyName); }
            set { SetProperty(BlockingCodePropertyName, value); }
        }

        public string Area2BlockingDesc
        {
            get { return GetProperty<string>(Area2BlockingDescPropertyName); }
            set { SetProperty(Area2BlockingDescPropertyName, value); }
        }
        #endregion
    }
}
