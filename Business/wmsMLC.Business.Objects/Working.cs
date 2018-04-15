namespace wmsMLC.Business.Objects
{
    public class Working : WMSBusinessObject
    {
        #region .  Consts  .
        public const string EntityType = "WmsWorking";

        public const string WORKERGROUPID_RPropertyName = "WORKERGROUPID_R";
        public const string WORKERID_RPropertyName = "WORKERID_R";
        public const string WORKID_RPropertyName = "WORKID_R";
        public const string WORKINGCOUNTPropertyName = "WORKINGCOUNT";
        public const string WORKINGCPVLPropertyName = "WORKINGCPVL";
        public const string WORKINGFROMPropertyName = "WORKINGFROM";
        public const string WORKINGIDPropertyName = "WORKINGID";
        public const string WORKINGMULTPropertyName = "WORKINGMULT";
        public const string WORKINGTILLPropertyName = "WORKINGTILL";
        public const string vWorkerFIOPropertyName = "vWorkerFIO";
        public const string TruckCodePropertyName = "TRUCKCODE_R";
        public const string WORKINGADDLPropertyName = "WORKINGADDL";
        #endregion .  Consts  .

        #region .  Properties  .
        public System.Decimal? WORKERGROUPID_R
        {
            get { return GetProperty<System.Decimal?>(WORKERGROUPID_RPropertyName); }
            set { SetProperty(WORKERGROUPID_RPropertyName, value); }
        }

        public System.Decimal? WORKERID_R
        {
            get { return GetProperty<System.Decimal?>(WORKERID_RPropertyName); }
            set { SetProperty(WORKERID_RPropertyName, value); }
        }

        public System.Decimal? WORKID_R
        {
            get { return GetProperty<System.Decimal?>(WORKID_RPropertyName); }
            set { SetProperty(WORKID_RPropertyName, value); }
        }

        public System.Decimal? WORKINGCOUNT
        {
            get { return GetProperty<System.Decimal?>(WORKINGCOUNTPropertyName); }
            set { SetProperty(WORKINGCOUNTPropertyName, value); }
        }

        public WMSBusinessCollection<WorkingCpv> WORKINGCPVL
        {
            get { return GetProperty<WMSBusinessCollection<WorkingCpv>>(WORKINGCPVLPropertyName); }
            set { SetProperty(WORKINGCPVLPropertyName, value); }
        }

        public System.DateTime? WORKINGFROM
        {
            get { return GetProperty<System.DateTime?>(WORKINGFROMPropertyName); }
            set { SetProperty(WORKINGFROMPropertyName, value); }
        }

        public System.Decimal? WORKINGID
        {
            get { return GetProperty<System.Decimal?>(WORKINGIDPropertyName); }
            set { SetProperty(WORKINGIDPropertyName, value); }
        }

        public System.Decimal WORKINGMULT
        {
            get { return GetProperty<System.Decimal>(WORKINGMULTPropertyName); }
            set { SetProperty(WORKINGMULTPropertyName, value); }
        }

        public System.DateTime? WORKINGTILL
        {
            get { return GetProperty<System.DateTime?>(WORKINGTILLPropertyName); }
            set { SetProperty(WORKINGTILLPropertyName, value); }
        }

        public string VWorkerFIO
        {
            get { return GetProperty<string>(vWorkerFIOPropertyName); }
        }

        public string TruckCode
        {
            get { return GetProperty<string>(TruckCodePropertyName); }
            set { SetProperty(TruckCodePropertyName, value); }
        }

        public bool WORKINGADDL
        {
            get { return GetProperty<bool>(WORKINGADDLPropertyName); }
            set { SetProperty(WORKINGADDLPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}