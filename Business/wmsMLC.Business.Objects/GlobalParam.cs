namespace wmsMLC.Business.Objects
{
    public class GlobalParam : WMSBusinessObject
    {
        #region .  Constants  .

        /// <summary>
        /// Определяет дефолтовое состояние галочки, разрешающей печатать этикетки в процессе приемки на форме приемки
        /// </summary>
        public const string PrintTEOnInput = "PrintTEOnInput";


        public const string GlobalParamCodePropertyName = "GLOBALPARAMCODE";
        public const string GlobalParam2EntityPropertyName = "GLOBALPARAM2ENTITY";
        public const string GlobalParamMustSetPropertyName = "GLOBALPARAMMUSTSET";
        public const string GlobalParamCountPropertyName = "GLOBALPARAMCOUNT";
        public const string GlobalParamDataTypePropertyName = "GLOBALPARAMDATATYPE";
        public const string GlobalParamLockedPropertyName = "GLOBALPARAMLOCKED";
        public const string GlobalParamDefaultPropertyName = "GLOBALPARAMDEFAULT";
        public const string GlobalParamNamePropertyName = "GLOBALPARAMNAME";
        public const string GlobalParamDescPropertyName = "GLOBALPARAMDESC";
        

        /*
        public const string UserInsPropertyName = "UserIns";
        public const string DateInsPropertyName = "DateIns";
        public const string UserUpdPropertyName = "UserUpd";
        public const string DateUpdPropertyName = "DateUpd";*/
        #endregion .  Constants  .

        #region .  Properties  .

        public string GlobalParamCode
        {
            get { return GetProperty<string>(GlobalParamCodePropertyName); }
            set { SetProperty(GlobalParamCodePropertyName, value); }
        }

        public bool GlobalParamMustSet
        {
            get { return GetProperty<bool>(GlobalParamMustSetPropertyName); }
            set { SetProperty(GlobalParamMustSetPropertyName, value); }
        }

        public decimal GlobalParamCount
        {
            get { return GetProperty<decimal>(GlobalParamCountPropertyName); }
            set { SetProperty(GlobalParamCountPropertyName, value); }
        }

        public string GlobalParamName
        {
            get { return GetProperty<string>(GlobalParamNamePropertyName); }
            set { SetProperty(GlobalParamNamePropertyName, value); }
        }
        #endregion .  Properties  .
    }
}
