using System;

namespace wmsMLC.Business.Objects
{
    public class MRUse : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MRUseStrategyTypePropertyName = "MRUSESTRATEGYTYPE";
        public const string MRUseStrategyPropertyName = "MRUSESTRATEGY";
        public const string MRUseStrategyValuePropertyName = "MRUSESTRATEGYVALUE";
        public const string MRUseParentPropertyName = "MRUSEPARENT";
        #endregion .  Constants  .

        #region .  Properties  .
       
        public string MRUseStrategyValue
        {
            get { return GetProperty<string>(MRUseStrategyValuePropertyName); }
            set { SetProperty(MRUseStrategyValuePropertyName, value); }
        }

        public string MRUseStrategyType
        {
            get { return GetProperty<string>(MRUseStrategyTypePropertyName); }
            set { SetProperty(MRUseStrategyTypePropertyName, value); }
        }

        public string MRUseStrategy
        {
            get { return GetProperty<string>(MRUseStrategyPropertyName); }
            set { SetProperty(MRUseStrategyPropertyName, value); }
        }

        public MRUseStrategyTypeSysEnum UseStrategyType
        {
            get
            {
                var prop = GetProperty<string>(MRUseStrategyTypePropertyName);
                return (MRUseStrategyTypeSysEnum)Enum.Parse(typeof(MRUseStrategyTypeSysEnum), prop);
            }
            set { SetProperty(MRUseStrategyTypePropertyName, value.ToString()); }
        }

        public MRUseStrategySysEnum UseStrategy
        {
            get
            {
                var prop = GetProperty<string>(MRUseStrategyPropertyName);
                return (MRUseStrategySysEnum)Enum.Parse(typeof(MRUseStrategySysEnum), prop);
            }
            set { SetProperty(MRUseStrategyPropertyName, value.ToString()); }
        }

        #endregion .  Properties  .
    }
}