namespace wmsMLC.Business.Objects
{
    public class PL : WMSBusinessObject
    {
        #region .  Constants  .
        public const string PLIDPropertyName = "PLID";
        public const string PLTypePropertyName = "PLTYPE";
        public const string StatusCode_rPropertyName = "STATUSCODE_R";
        public const string MPLCode_rPropertyName = "MPLCODE_R";
        public const string PLPosLPropertyName = "PLPOSL";
        #endregion .  Constants  .

        #region .  Properties  .

        public decimal? PLID
        {
            get { return GetProperty<decimal?>(PLIDPropertyName); }
            set { SetProperty(PLIDPropertyName, value); }
        }

        public WMSBusinessCollection<PLPos> PLPosL
        {
            get { return GetProperty<WMSBusinessCollection<PLPos>>(PLPosLPropertyName); }
            set { SetProperty(PLPosLPropertyName, value); }
        }

        public string PLType
        {
            get { return GetProperty<string>(PLTypePropertyName); }
            set { SetProperty(PLTypePropertyName, value); }
        }

        public string StatusCode_r
        {
            get { return GetProperty<string>(StatusCode_rPropertyName); }
            set { SetProperty(StatusCode_rPropertyName, value); }
        }

        public string MPLCode_r
        {
            get { return GetProperty<string>(MPLCode_rPropertyName); }
            set { SetProperty(MPLCode_rPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}