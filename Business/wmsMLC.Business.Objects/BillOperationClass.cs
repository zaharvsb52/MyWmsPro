namespace wmsMLC.Business.Objects
{
    public class BillOperationClass : WMSBusinessObject
    {
        public const string OperationClassCodePropertyName = "OPERATIONCLASSCODE";
        public const string OperationClassNamePropertyName = "OPERATIONCLASSNAME";

        public string OperationClassCode
        {
            get { return GetProperty<string>(OperationClassCodePropertyName); }
            set { SetProperty(OperationClassCodePropertyName, value); }
        }

        public string OperationClassName
        {
            get { return GetProperty<string>(OperationClassNamePropertyName); }
            set { SetProperty(OperationClassNamePropertyName, value); }
        }
    }
}