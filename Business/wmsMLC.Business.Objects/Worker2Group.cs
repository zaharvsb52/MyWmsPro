using System;

namespace wmsMLC.Business.Objects
{
    public class Worker2Group : WMSBusinessObject
    {
        public const string WORKER2GROUPCPVLPropertyName = "WORKER2GROUPCPVL";
        public const string WORKER2GROUPIDPropertyName = "WORKER2GROUPID";
        public const string WORKER2GROUPWORKERGROUPIDPropertyName = "WORKER2GROUPWORKERGROUPID";
        public const string WORKER2GROUPWORKERIDPropertyName = "WORKER2GROUPWORKERID";

//        public WORKER2GROUPCPV? WORKER2GROUPCPVL
//        {
//            get { return GetProperty<WORKER2GROUPCPV?>(WORKER2GROUPCPVLPropertyName); }
//            set { SetProperty(WORKER2GROUPCPVLPropertyName, value); }
//        }

        public Decimal? WORKER2GROUPID
        {
            get { return GetProperty<Decimal?>(WORKER2GROUPIDPropertyName); }
            set { SetProperty(WORKER2GROUPIDPropertyName, value); }
        }

        public Decimal? WORKER2GROUPWORKERGROUPID
        {
            get { return GetProperty<Decimal?>(WORKER2GROUPWORKERGROUPIDPropertyName); }
            set { SetProperty(WORKER2GROUPWORKERGROUPIDPropertyName, value); }
        }

        public Decimal? WORKER2GROUPWORKERID
        {
            get { return GetProperty<Decimal?>(WORKER2GROUPWORKERIDPropertyName); }
            set { SetProperty(WORKER2GROUPWORKERIDPropertyName, value); }
        }
    }
}