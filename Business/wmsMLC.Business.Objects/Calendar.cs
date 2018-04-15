using System;

namespace wmsMLC.Business.Objects
{
    public class Calendar : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CALENDARPropertyName = "TENTCALENDAR";
        public const string CALENDARDATEPropertyName = "CALENDARDATE";
        public const string CALENDARDAYOFWEEKPropertyName = "CALENDARDAYOFWEEK";
        public const string CALENDARIDPropertyName = "CALENDARID";
        public const string CALENDARTIMEFROMPropertyName = "CALENDARTIMEFROM";
        public const string CALENDARTIMETILLPropertyName = "CALENDARTIMETILL";
        public const string CALENDARTYPEPropertyName = "CALENDARTYPE";
        public const string VCALENDARDAYOFWEEKPropertyName = "VCALENDARDAYOFWEEK";
        public const string VCALENDARTYPEPropertyName = "VCALENDARTYPE";
        #endregion

       public DateTime? CalendarDate
        {
            get { return GetProperty<DateTime?>(CALENDARDATEPropertyName); }
            set { SetProperty(CALENDARDATEPropertyName, value); }
        }

       public DateTime CalendarTimeFrom
        {
            get { return GetProperty<DateTime>(CALENDARTIMEFROMPropertyName); }
            set { SetProperty(CALENDARTIMEFROMPropertyName, value); }
        }
        public DateTime CalendarTimeTill
        {
            get { return GetProperty<DateTime>(CALENDARTIMETILLPropertyName); }
            set { SetProperty(CALENDARTIMETILLPropertyName, value); }
        }
    }
}