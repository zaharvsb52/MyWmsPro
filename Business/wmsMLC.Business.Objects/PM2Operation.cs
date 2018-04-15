namespace wmsMLC.Business.Objects
{
    /// <summary>
    /// Сущность 'Операции к менеджеру товара'.
    /// </summary>
    public class PM2Operation : WMSBusinessObject
    {
        #region .  Constants  .

        public const string PM2OperationOperationCodePropertyName = "PM2OPERATIONOPERATIONCODE";
        public const string PM2OperationPMCodePropertyName = "PM2OPERATIONPMCODE";
        
        #endregion .  Constants  .

        #region .  Properties  .

        public string OperationCode_r
        {
            get { return GetProperty<string>(PM2OperationOperationCodePropertyName); }
            set { SetProperty(PM2OperationOperationCodePropertyName, value); }
        }

        public string PM2OperationPMCode
        {
            get { return GetProperty<string>(PM2OperationPMCodePropertyName); }
            set { SetProperty(PM2OperationPMCodePropertyName, value); }
        }

        #endregion .  Properties  .
    }
}