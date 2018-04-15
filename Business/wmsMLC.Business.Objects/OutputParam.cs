namespace wmsMLC.Business.Objects
{
    public class OutputParam : WMSBusinessObject
    {
        #region .  Constants  .
        public const string OutputParamCodePropertyName = "OUTPUTPARAMCODE";
        public const string OutputParamValuePropertyName = "OUTPUTPARAMVALUE";
        public const string OutputParamSubvaluePropertyName = "OUTPUTPARAMSUBVALUE";
        public const string OutputParamTypePropertyName = "OUTPUTPARAMTYPE";
        
        #endregion .  Constants  .

        #region .  Properties  .
        public string OutputParamCode
        {
            get { return GetProperty<string>(OutputParamCodePropertyName); }
            set { SetProperty(OutputParamCodePropertyName, value); }
        }

        public string OutputParamValue
        {
            get { return GetProperty<string>(OutputParamValuePropertyName); }
            set { SetProperty(OutputParamValuePropertyName, value); }
        }

        public string OutputParamSubvalue
        {
            get { return GetProperty<string>(OutputParamSubvaluePropertyName); }
            set { SetProperty(OutputParamSubvaluePropertyName, value); }
        }

        public string OutputParamType
        {
            get { return GetProperty<string>(OutputParamTypePropertyName); }
            set { SetProperty(OutputParamTypePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}