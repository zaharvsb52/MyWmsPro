namespace wmsMLC.Business.Objects
{
    public class PLPos : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PLPosIDPropertyName = "PLPOSID";
        public const string PLID_rPropertyName = "PLID_r";
        public const string StatusCode_rPropertyName = "STATUSCODE_R";
        public const string PlaceCode_rPropertyName = "PLACECODE_R";
        public const string TECode_rPropertyName = "TECODE_R";
        public const string PLPosSortPropertyName = "PLPOSSORT";
        public const string SKUID_rPropertyName = "SKUID_R";
        public const string PLPosCountSKUPlanPropertyName = "PLPOSCOUNTSKUPLAN";
        public const string PLPosCountSKUFactPropertyName = "PLPOSCOUNTSKUFACT";

        #endregion .  Constants  .

        #region .  Properties  .

        public decimal? PLPosID
        {
            get { return GetProperty<decimal?>(PLPosIDPropertyName); }
            set { SetProperty(PLPosIDPropertyName, value); }
        }

        public decimal? PLID_R
        {
            get { return GetProperty<decimal?>(PLID_rPropertyName); }
            set { SetProperty(PLID_rPropertyName, value); }
        }

        public string StatusCode_r
        {
            get { return GetProperty<string>(StatusCode_rPropertyName); }
            set { SetProperty(StatusCode_rPropertyName, value); }
        }

        public string PlaceCode_r
        {
            get { return GetProperty<string>(PlaceCode_rPropertyName); }
            set { SetProperty(PlaceCode_rPropertyName, value); }
        }

        public string TECode_r
        {
            get { return GetProperty<string>(TECode_rPropertyName); }
            set { SetProperty(TECode_rPropertyName, value); }
        }

        public decimal? PLPosSort
        {
            get { return GetProperty<decimal?>(PLPosSortPropertyName); }
            set { SetProperty(PLPosSortPropertyName, value); }
        }

        public decimal? SKUID_r
        {
            get { return GetProperty<decimal?>(SKUID_rPropertyName); }
            set { SetProperty(SKUID_rPropertyName, value); }
        }

        public decimal? PLPosCountSKUPlan
        {
            get { return GetProperty<decimal?>(PLPosCountSKUPlanPropertyName); }
            set { SetProperty(PLPosCountSKUPlanPropertyName, value); }
        }

        public decimal? PLPosCountSKUFact
        {
            get { return GetProperty<decimal?>(PLPosCountSKUFactPropertyName); }
            set { SetProperty(PLPosCountSKUFactPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}