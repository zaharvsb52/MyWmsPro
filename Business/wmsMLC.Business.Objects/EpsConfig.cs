namespace wmsMLC.Business.Objects
{
    public class EpsConfig : WMSBusinessObject
    {
        #region .  Constants  .
        public const string EpsConfig2EntityParamCodePropertyName = "EPSCONFIG2ENTITY";
        public const string EpsConfigParamCodePropertyName = "EPSCONFIGPARAMCODE";
        public const string EpsConfigLockedPropertyName = "EPSCONFIGLOCKED";
        public const string EpsConfigStrongUsePropertyName = "EPSCONFIGSTRONGUSE";
        public const string EpsConfigValuePropertyName = "EPSCONFIGVALUE";
        public const string EpsConfigKeyPropertyName = "EPSCONFIGKEY";
        #endregion .  Constants  .

        #region .  Properties  .
        public string EpsConfig2Entity
        {
            get { return GetProperty<string>(ChangePropertyName(EpsConfig2EntityParamCodePropertyName)); }
            set { SetProperty(ChangePropertyName(EpsConfig2EntityParamCodePropertyName), value); }
        }

        public string EpsConfigParamCode
        {
            get { return GetProperty<string>(ChangePropertyName(EpsConfigParamCodePropertyName)); }
            set { SetProperty(ChangePropertyName(EpsConfigParamCodePropertyName), value); }
        }

        public bool EpsConfigLocked
        {
            get { return GetProperty<bool>(ChangePropertyName(EpsConfigLockedPropertyName)); }
            set { SetProperty(ChangePropertyName(EpsConfigLockedPropertyName), value); }
        }

        public bool EpsConfigStrongUse
        {
            get { return GetProperty<bool>(ChangePropertyName(EpsConfigStrongUsePropertyName)); }
            set { SetProperty(ChangePropertyName(EpsConfigStrongUsePropertyName), value); }
        }

        public string EpsConfigValue
        {
            get { return GetProperty<string>(ChangePropertyName(EpsConfigValuePropertyName)); }
            set { SetProperty(ChangePropertyName(EpsConfigValuePropertyName), value); }
        }

        public string EpsConfigKey
        {
            get { return GetProperty<string>(ChangePropertyName(EpsConfigKeyPropertyName)); }
            set { SetProperty(ChangePropertyName(EpsConfigKeyPropertyName), value); }
        }
        #endregion .  Properties  .

        /// <summary>
        /// Преобразование базового названия свойства.
        /// </summary>
        protected virtual string ChangePropertyName(string basepropertyname)
        {
            return GetType() == typeof(EpsConfig) ? basepropertyname : string.Format("{0}_{1}", basepropertyname, GetType().Name).ToUpper();
        }
    }
}