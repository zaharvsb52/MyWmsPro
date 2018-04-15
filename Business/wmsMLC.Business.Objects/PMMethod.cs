namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Сущность 'Способы обработки'.
    /// </summary>
    public class PMMethod : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PMMETHODNAMEPropertyName = "PMMETHODNAME";

        #endregion .  Constants  .

        #region .  Properties  .

        public string PMMETHODNAME
        {
            get { return GetProperty<string>(PMMETHODNAMEPropertyName); }
            set { SetProperty(PMMETHODNAMEPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}