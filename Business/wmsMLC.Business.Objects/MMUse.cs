using System;

namespace wmsMLC.Business.Objects
{
    public class MMUse : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MMUseStrategyPropertyName = "MMUSESTRATEGY";
        public const string MMUseStrategyValuePropertyName = "MMUSESTRATEGYVALUE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string MMUseStrategy
        {
            get { return GetProperty<string>(MMUseStrategyPropertyName); }
            set { SetProperty(MMUseStrategyPropertyName, value); }
        }
        public string MMUseStrategyValue
        {
            get { return GetProperty<string>(MMUseStrategyValuePropertyName); }
            set { SetProperty(MMUseStrategyValuePropertyName, value); }
        }

        public MovingUseStrategySysEnum UseStrategy
        {
            get
            {
                var prop = GetProperty<string>(MMUseStrategyPropertyName);
                return (MovingUseStrategySysEnum)Enum.Parse(typeof(MovingUseStrategySysEnum), prop);
            }
            set { SetProperty(MMUseStrategyPropertyName, value.ToString()); }
        }
        #endregion .  Properties  .

    }
}