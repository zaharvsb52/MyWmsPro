namespace wmsMLC.Business.Objects
{
    public class IWBPosQLFDetailDesc : WMSBusinessObject
    {

        public const string QlfDetailCodePropertyName = "QLFDETAILCODE_R";
        public const string QlfDetailDescPropertyName = "QLFDETAILDESC";

        public string QlfDetailDesc
        {
            get { return GetProperty<string>(QlfDetailDescPropertyName); }
            set { SetProperty(QlfDetailDescPropertyName, value); }
        }

        public string QlfDetailCode
        {
            get { return GetProperty<string>(QlfDetailCodePropertyName); }
            set { SetProperty(QlfDetailCodePropertyName, value); }
        }
    }
}