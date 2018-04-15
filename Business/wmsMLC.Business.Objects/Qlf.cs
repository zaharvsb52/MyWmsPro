namespace wmsMLC.Business.Objects
{
    public class Qlf : WMSBusinessObject
    {
        public const string QLFTypePropertyName = "QLFTYPE";
        public const string QLFCodePropertyName = "QLFCODE";
        public const string QLFNamePropertyName = "QLFNAME";

        public string QLFType
        {
            get { return GetProperty<string>(QLFTypePropertyName); }
            set { SetProperty(QLFTypePropertyName, value); }
        }
        public string QLFCode
        {
            get { return GetProperty<string>(QLFCodePropertyName); }
            set { SetProperty(QLFCodePropertyName, value); }
        }
        public string QLFName
        {
            get { return GetProperty<string>(QLFNamePropertyName); }
            set { SetProperty(QLFNamePropertyName, value); }
        }
    }
}