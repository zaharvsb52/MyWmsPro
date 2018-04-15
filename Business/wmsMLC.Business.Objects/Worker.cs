using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    public class Worker : WMSBusinessObject
    {
        #region . Constants .

        public const string WorkerIDPropertyName = "WORKERID";
        public const string WorkerLastNamePropertyName = "WORKERLASTNAME";
        public const string WorkerNamePropertyName = "WORKERNAME";
        public const string WorkerMiddleNamePropertyName = "WORKERMIDDLENAME";
        public const string WorkerEmployeePropertyName = "WORKEREMPLOYEE";
        public const string WorkerPhoneWorkPropertyName = "WORKERPHONEWORK";
        public const string WorkerPhoneMobilePropertyName = "WORKERPHONEMOBILE";
        public const string WorkerEmailWorkPropertyName = "WORKEREMAILWORK";
        public const string WorkerEmailPersonalPropertyName = "WORKEREMAILPERSONAL";
        public const string WorkerAddressPropertyName = "WORKERADDRESS";
        public const string WorkerPassLPropertyName = "WORKERPASSL";
        public const string WorkerFIOPropertyName = "WORKERFIO";
        public const string UserCodePropertyName = "USERCODE_R";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        //public const string GlobalParamValPropertyName = "GLOBALPARAMVAL";

        #endregion

        #region . Properties .
        //public WMSBusinessCollection<WorkerGpv> GlobalParamVal
        //{
        //    get { return GetProperty<WMSBusinessCollection<WorkerGpv>>(GlobalParamValPropertyName); }
        //    set { SetProperty(GlobalParamValPropertyName, value); }
        //}

        public WMSBusinessCollection<WorkerCpv> CUSTOMPARAMVAL
        {
            get { return GetProperty<WMSBusinessCollection<WorkerCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }

        public WMSBusinessCollection<AddressBook> WorkerAddress
        {
            get { return GetProperty<WMSBusinessCollection<AddressBook>>(WorkerAddressPropertyName); }
            set { SetProperty(WorkerAddressPropertyName, value); }
        }

        [XmlNotIgnore]
        public WMSBusinessCollection<WorkerPass> WorkerPassL
        {
            get { return GetProperty<WMSBusinessCollection<WorkerPass>>(WorkerPassLPropertyName); }
            set { SetProperty(WorkerPassLPropertyName, value); }
        }

        public string WorkerFIO
        {
            get { return GetProperty<string>(WorkerFIOPropertyName); }
            set { SetProperty(WorkerFIOPropertyName, value); }
        }

        public string UserCode
        {
            get { return GetProperty<string>(UserCodePropertyName); }
            set { SetProperty(UserCodePropertyName, value); }
        }
        #endregion
    }
}