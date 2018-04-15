namespace wmsMLC.Business.Objects
{
    public class BillWorkAct2Op2C : WMSBusinessObject
    {
        #region .  Constants  .

        public const string WorkActIDPropertyName = "BILLWORKACT2OP2CWORKACTID";

        #endregion

        #region .  Properties  .

        public decimal WorkActID
        {
            get { return GetProperty<decimal>(WorkActIDPropertyName); }
            set { SetProperty(WorkActIDPropertyName, value); }
        }

        #endregion
    }
}