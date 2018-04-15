namespace wmsMLC.Business.Objects
{
    public class MPLSelect : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MandantIdPropertyName = "MANDANTID";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? MandantId
        {
            get { return GetProperty<decimal?>(MandantIdPropertyName); }
            set { SetProperty(MandantIdPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}