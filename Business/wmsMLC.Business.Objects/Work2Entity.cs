namespace wmsMLC.Business.Objects
{
    public class Work2Entity : WMSBusinessObject
    {
        #region .  Consts  .
        public const string Work2EntityIdPropertyName = "WORK2ENTITYID";
        public const string WorkID_rPropertyName = "WORKID_R";
        public const string Work2EntityEntityPropertyName = "WORK2ENTITYENTITY";
        public const string Work2EntityKeyPropertyName = "WORK2ENTITYKEY";
        public const string W2E2WorkingLPropertyName = "W2E2WORKINGL";
        #endregion

        #region .  Properties  .
        public decimal? Work2EntityId
        {
            get { return GetProperty<decimal?>(Work2EntityIdPropertyName); }
            set { SetProperty(Work2EntityIdPropertyName, value); }
        }

        public decimal? WorkID_r
        {
            get { return GetProperty<decimal?>(WorkID_rPropertyName); }
            set { SetProperty(WorkID_rPropertyName, value); }
        }
        

        public string Work2EntityEntity
        {
            get { return GetProperty<string>(Work2EntityEntityPropertyName); }
            set { SetProperty(Work2EntityEntityPropertyName, value); }
        }

        public string Work2EntityKey
        {
            get { return GetProperty<string>(Work2EntityKeyPropertyName); }
            set { SetProperty(Work2EntityKeyPropertyName, value); }
        }

        public WMSBusinessCollection<W2E2Working> W2E2WorkingL
        {
            get { return GetProperty<WMSBusinessCollection<W2E2Working>>(W2E2WorkingLPropertyName); }
            set { SetProperty(W2E2WorkingLPropertyName, value); }
        }
        #endregion
    }
}