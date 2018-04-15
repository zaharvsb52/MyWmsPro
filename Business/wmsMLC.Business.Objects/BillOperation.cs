namespace wmsMLC.Business.Objects
{
    public class BillOperation : WMSBusinessObject
    {
        public const string OperationCodePropertyName = "OperationCode";
        public const string OperationClassCodePropertyName = "OPERATIONCLASSCODE_R";
        public const string OperationNamePropertyName = "OperationName";

        #region .  Properties  .
        public string OperationCode
        {
            get { return GetProperty<string>(OperationCodePropertyName); }
            set { SetProperty(OperationCodePropertyName, value); }
        }
        public string OperationClassCode
        {
            get { return GetProperty<string>(OperationClassCodePropertyName); }
            set { SetProperty(OperationClassCodePropertyName, value); }
        }
        public string OperationName
        {
            get { return GetProperty<string>(OperationNamePropertyName); }
            set { SetProperty(OperationNamePropertyName, value); }
        }
        #endregion
    }
}