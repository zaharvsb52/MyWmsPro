namespace wmsMLC.Business.Objects
{
    public class EpsTask : WMSBusinessObject
    {
        #region .  Constants  .
        public const string TaskTypePropertyName = "TASKTYPE";
        public const string TaskLockedPropertyName = "TASKLOCKED";
        public const string Task2JobPropertyName = "TASK2JOB";
        public const string ConfigTskPropertyName = "CONFIGTSK";
        #endregion .  Constants  .

        #region .  Properties  .
        public string TaskType
        {
            get { return GetProperty<string>(TaskTypePropertyName); }
            set { SetProperty(TaskTypePropertyName, value); }
        }

        public bool TaskLocked
        {
            get { return GetProperty<bool>(TaskLockedPropertyName); }
            set { SetProperty(TaskLockedPropertyName, value); }
        }

        public WMSBusinessCollection<EpsTask2Job> Task2Job
        {
            get { return GetProperty<WMSBusinessCollection<EpsTask2Job>>(Task2JobPropertyName); }
            set { SetProperty(Task2JobPropertyName, value); }
        }

        public WMSBusinessCollection<EpsTaskCfg> ConfigTsk
        {
            get { return GetProperty<WMSBusinessCollection<EpsTaskCfg>>(ConfigTskPropertyName); }
            set { SetProperty(ConfigTskPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}