namespace wmsMLC.Business.Objects
{
    public class Min : WMSBusinessObject
    {
        public const string CustomParamValPropertyName = "CUSTOMPARAMVAL";

        public WMSBusinessCollection<MinCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<MinCpv>>(CustomParamValPropertyName); }
            set { SetProperty(CustomParamValPropertyName, value); }
        }
    }
}