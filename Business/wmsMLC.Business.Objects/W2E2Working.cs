namespace wmsMLC.Business.Objects
{
    public class W2E2Working : WMSBusinessObject
    {
        #region .  Consts  .
        public const string WorkingIdName = "WORKINGID_R";
        public const string Work2EntityIdName = "WORK2ENTITYID_R"; 
        #endregion

        #region .  Properties  .
        public decimal? WorkingId
        {
            get { return GetProperty<decimal?>(WorkingIdName); }
            set { SetProperty(WorkingIdName, value); }
        }

        public decimal? Work2EntityId
        {
            get { return GetProperty<decimal?>(Work2EntityIdName); }
            set { SetProperty(Work2EntityIdName, value); }
        }

        #endregion
    }
}

