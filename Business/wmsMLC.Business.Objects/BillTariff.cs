namespace wmsMLC.Business.Objects
{
    public class BillTariff : WMSBusinessObject
    {
        #region . Constants .
        public const string BILLTariffValuePropertyName = "TARIFFVALUE";
        #endregion . Constants .

        #region . Properties .

        public double Value
        {
            get { return GetProperty<double>(BILLTariffValuePropertyName); }
            set { SetProperty(BILLTariffValuePropertyName, value); }
        }

        #endregion . Properties .
    }
}