namespace wmsMLC.Business.Objects
{
    public class GlobalParamValue : WMSBusinessObject
    {
        #region .  Constants  .
        public const string GParamIDPropertyName = "GPARAMID";
        public const string GParamVal2EntityPropertyName = "GPARAMVAL2ENTITY";
        public const string GlobalParamCodePropertyName = "GLOBALPARAMCODE_R";
        public const string GParamValKeyPropertyName = "GPARAMVALKEY";
        public const string GparamValValuePropertyName = "GPARAMVALVALUE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string GlobalParamCode_R
        {
            get { return GetProperty<string>(ChangePropertyName(GlobalParamCodePropertyName)); }
            set { SetProperty(ChangePropertyName(GlobalParamCodePropertyName), value); }
        }

        public string GparamValValue
        {
            get { return GetProperty<string>(ChangePropertyName(GparamValValuePropertyName)); }
            set { SetProperty(ChangePropertyName(GparamValValuePropertyName), value); }
        }

        public string GParamVal2Entity
        {
            get { return GetProperty<string>(ChangePropertyName(GParamVal2EntityPropertyName)); }
            set { SetProperty(ChangePropertyName(GParamVal2EntityPropertyName), value); }
        }

        public string GParamValKey
        {
            get { return GetProperty<string>(ChangePropertyName(GParamValKeyPropertyName)); }
            set { SetProperty(ChangePropertyName(GParamValKeyPropertyName), value); }
        }

        public decimal GParamID
        {
            get { return GetProperty<decimal>(ChangePropertyName(GParamIDPropertyName)); }
            set { SetProperty(ChangePropertyName(GParamIDPropertyName), value); }
        }
        #endregion .  Properties  .

        /// <summary>
        /// Преобразование базового названия свойства.
        /// </summary>
        protected virtual string ChangePropertyName(string basePropertyName)
        {
            return GetType() == typeof(GlobalParamValue)
                ? basePropertyName 
                : string.Format("{0}_{1}", basePropertyName, GetType().Name).ToUpper();
        }
    }
}
