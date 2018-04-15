namespace wmsMLC.Business.Objects
{
    public class EpsJob : WMSBusinessObject
    {
        #region .  Constants  .
        public const string JobLockedPropertyName = "JOBLOCKED";
        public const string JobHandlerPropertyName = "JOBHANDLER";
        public const string Job2TaskPropertyName = "JOB2TASK";
        public const string ConfigEpsPropertyName = "CONFIGEPS";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        #endregion .  Constants  .

        #region .  Properties  .
        public bool JobLocked
        {
            get { return GetProperty<bool>(JobLockedPropertyName); }
            set { SetProperty(JobLockedPropertyName, value); }
        }

        public decimal JobHandler
        {
            get { return GetProperty<decimal>(JobHandlerPropertyName); }
            set { SetProperty(JobLockedPropertyName, value); }
        }

        public WMSBusinessCollection<EpsTask2Job> Job2Task
        {
            get { return GetProperty<WMSBusinessCollection<EpsTask2Job>>(Job2TaskPropertyName); }
            set { SetProperty(Job2TaskPropertyName, value); }
        }

        public WMSBusinessCollection<EpsJobCfg> ConfigEps
        {
            get { return GetProperty<WMSBusinessCollection<EpsJobCfg>>(ConfigEpsPropertyName); }
            set { SetProperty(ConfigEpsPropertyName, value); }
        }

        public WMSBusinessCollection<EpsJobCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<EpsJobCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}