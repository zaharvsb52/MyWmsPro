using System;

namespace wmsMLC.Business.Objects
{
    public class Schedule : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ScheduleIdPropertyName = "SCHEDULEID";
        public const string ScheduleDateBeginPropertyName = "SCHEDULEDATEBEGIN";
        public const string ScheduleDateEndPropertyName = "SCHEDULEDATEEND";
        public const string ScheduleCronPropertyName = "SCHEDULECRON";
        public const string JobCode_RPropertyName = "JOBCODE_R";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? ScheduleId
        {
            get { return GetProperty<decimal?>(ScheduleIdPropertyName); }
            set { SetProperty(ScheduleIdPropertyName, value); }
        }

        public DateTime? ScheduleDateBegin
        {
            get { return GetProperty<DateTime?>(ScheduleDateBeginPropertyName); }
            set { SetProperty(ScheduleDateBeginPropertyName, value); }
        }

        public DateTime? ScheduleDateEnd
        {
            get { return GetProperty<DateTime?>(ScheduleDateEndPropertyName); }
            set { SetProperty(ScheduleDateEndPropertyName, value); }
        }

        public string ScheduleCron
        {
            get { return GetProperty<string>(ScheduleCronPropertyName); }
            set { SetProperty(ScheduleCronPropertyName, value); }
        }

        public string JobCode_R
        {
            get { return GetProperty<string>(JobCode_RPropertyName); }
            set { SetProperty(JobCode_RPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
