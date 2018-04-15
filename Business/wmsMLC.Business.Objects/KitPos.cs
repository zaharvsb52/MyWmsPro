namespace wmsMLC.Business.Objects
{
    public class KitPos : WMSBusinessObject
    {
        #region .  Constants  .

        public const string KitPosIDPropertyName = "KITPOSID";
        public const string KitPosCodeRPropertyName = "KITCODE_R";
        public const string KitPosSKUIDRPropertyName = "SKUID_R";
        public const string KitPosCountPropertyName = "KITPOSCOUNT";
        public const string KitPosPriorityPropertyName = "KITPOSPRIORITY";
        public const string KitPosArtNamePropertyName = "KITPOSARTNAME";
        public const string KitPosMeasurePropertyName = "KITPOSMEASURE";

        #endregion

        #region .  Properties  .

        public decimal? KitPosID
        {
            get { return GetProperty<decimal?>(KitPosIDPropertyName); }
            set { SetProperty(KitPosIDPropertyName, value); }
        }
        public string KitPosCodeR
        {
            get { return GetProperty<string>(KitPosCodeRPropertyName); }
            set { SetProperty(KitPosCodeRPropertyName, value); }
        }
        public decimal KitPosSKUIDR
        {
            get { return GetProperty<decimal>(KitPosSKUIDRPropertyName); }
            set { SetProperty(KitPosSKUIDRPropertyName, value); }
        }
        public decimal KitPosCount
        {
            get { return GetProperty<decimal>(KitPosCountPropertyName); }
            set { SetProperty(KitPosCountPropertyName, value); }
        }
        public decimal KitPosPriority
        {
            get { return GetProperty<decimal>(KitPosPriorityPropertyName); }
            set { SetProperty(KitPosPriorityPropertyName, value); }
        }
        public string KitPosArtName
        {
            get { return GetProperty<string>(KitPosArtNamePropertyName); }
            set { SetProperty(KitPosArtNamePropertyName, value); }
        }
        public string KitPosMeasure
        {
            get { return GetProperty<string>(KitPosMeasurePropertyName); }
            set { SetProperty(KitPosMeasurePropertyName, value); }
        }

        #endregion
    }
}