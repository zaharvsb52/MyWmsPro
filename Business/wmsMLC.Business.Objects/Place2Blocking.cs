namespace wmsMLC.Business.Objects
{
    public class Place2Blocking : WMSBusinessObject
    {
        #region .  Constants  .
        public const string Place2BlockingPlaceCodePropertyName = "PLACE2BLOCKINGPLACECODE";
        public const string Place2BlockingDescPropertyName = "PLACE2BLOCKINGDESC";
        public const string Place2BlockingBlockingCodePropertyName = "PLACE2BLOCKINGBLOCKINGCODE";
        public const string Place2BlockingIdPropertyName = "PLACE2BLOCKINGID";
        public const string VBLOCKINGNAMEPropertyName = "VBLOCKINGNAME";
        public const string VPLACENAMEPropertyName = "VPLACENAME";                                                                                                                                                                       

        #endregion .  Constants  .

        #region .  Properties  .
        public string Place2BlockingPlaceCode
        {
            get { return GetProperty<string>(Place2BlockingPlaceCodePropertyName); }
            set { SetProperty(Place2BlockingPlaceCodePropertyName, value); }
        }

        public string Place2BlockingDesc
        {
            get { return GetProperty<string>(Place2BlockingDescPropertyName); }
            set { SetProperty(Place2BlockingDescPropertyName, value); }
        }

        public string Place2BlockingBlockingCode
        {
            get { return GetProperty<string>(Place2BlockingBlockingCodePropertyName); }
            set { SetProperty(Place2BlockingBlockingCodePropertyName, value); }
        }
        public decimal? Place2BlockingId
        {
            get { return GetProperty<decimal?>(Place2BlockingIdPropertyName); }
            set { SetProperty(Place2BlockingIdPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}