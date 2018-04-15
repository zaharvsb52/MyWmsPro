using System;

namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Сущность 'Конфигурация менеджера товара'.
    /// </summary>
    public class PMConfig : WMSBusinessObject
    {
        #region .  Constants  .
        public const string PM2OperationCodePropertyName = "PM2OPERATIONCODE_R";
        public const string ObjectEntitycode_RPropertyName = "OBJECTENTITYCODE_R";
        public const string ObjectNamePropertyName = "OBJECTNAME_R";
        public const string PMMethodCodePropertyName = "PMMETHODCODE_R";
        public const string PMCONFIGBYPRODUCTPropertyName = "PMCONFIGBYPRODUCT";
        public const string PMCONFIGINPUTMASKPropertyName = "PMCONFIGINPUTMASK";
        public const string PMCONFIGINPUTMASSPropertyName = "PMCONFIGINPUTMASS";

        #endregion .  Constants  .

        #region .  Properties  .
        public string PM2OperationCode_r
        {
            get { return GetProperty<string>(PM2OperationCodePropertyName); }
            set { SetProperty(PM2OperationCodePropertyName, value); }
        }

        public string ObjectEntitycode_R
        {
            get { return GetProperty<string>(ObjectEntitycode_RPropertyName); }
            set { SetProperty(ObjectEntitycode_RPropertyName, value); }
        }

        public string ObjectName_r
        {
            get { return GetProperty<string>(ObjectNamePropertyName); }
            set { SetProperty(ObjectNamePropertyName, value); }
        }

        public string MethodCode_r
        {
            get { return GetProperty<string>(PMMethodCodePropertyName); }
            set { SetProperty(PMMethodCodePropertyName, value); }
        }

        public bool? PMCONFIGBYPRODUCT
        {
            get { return GetProperty<bool?>(PMCONFIGBYPRODUCTPropertyName); }
            set { SetProperty(PMCONFIGBYPRODUCTPropertyName, value); }
        }

        public string PMCONFIGINPUTMASK
        {
            get { return GetProperty<string>(PMCONFIGINPUTMASKPropertyName); }
            set { SetProperty(PMCONFIGINPUTMASKPropertyName, value); }
        }
        
        public bool? PMCONFIGINPUTMASS
        {
            get { return GetProperty<bool?>(PMCONFIGINPUTMASSPropertyName); }
            set { SetProperty(PMCONFIGINPUTMASSPropertyName, value); }
        }


        public Func<object, string> ValidatorHandle { get; set; }
        #endregion .  Properties  .
    }
}