namespace wmsMLC.Business.Objects
{
    public class MIUse : WMSBusinessObject
    {
        #region .  Constants  .
        public const string MIUSESTRATEGYTYPEPropertyName = "MIUSESTRATEGYTYPE";
        public const string MIUSEGROUPCODEPropertyName = "MIUSEGROUPCODE";
        public const string MIUseStrategyValuePropertyName = "MIUSESTRATEGYVALUE";
        public const string MIUseOrderTypePropertyName = "MIUSEORDERTYPE";
        public const string MIUseToCharPropertyName = "MIUSETOCHAR";
        public const string MIUseFilterPropertyName = "MIUSEFILTER";
        #endregion

        #region .  Properties  .
        public string StrategyType
        {
            get { return GetProperty<string>(MIUSESTRATEGYTYPEPropertyName); }
            set { SetProperty(MIUSESTRATEGYTYPEPropertyName, value); }
        }

        public string OrderType
        {
            get { return GetProperty<string>(MIUseOrderTypePropertyName); }
            set { SetProperty(MIUseOrderTypePropertyName, value); }
        }

        public string GroupCode
        {
            get { return GetProperty<string>(MIUSEGROUPCODEPropertyName); }
            set { SetProperty(MIUSEGROUPCODEPropertyName, value); }
        }

        public string MIUseStrategyValue
        {
            get { return GetProperty<string>(MIUseStrategyValuePropertyName); }
            set { SetProperty(MIUseStrategyValuePropertyName, value); }
        }

        public bool? ToChar
        {
            get { return GetProperty<bool?>(MIUseToCharPropertyName); }
            set { SetProperty(MIUseToCharPropertyName, value); }
        }

        public string Filter
        {
            get { return GetProperty<string>(MIUseFilterPropertyName); }
            set { SetProperty(MIUseFilterPropertyName, value); }
        }
        #endregion
    }
}