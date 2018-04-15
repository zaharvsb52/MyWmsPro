namespace wmsMLC.Business.Objects
{
    public class Client : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ClientCodePropertyName = "CLIENTCODE";
        public const string ClientNamePropertyName = "CLIENTNAME";
        public const string ClientMacPropertyName = "CLIENTMAC";
        public const string ClientIpPropertyName = "CLIENTIP";
        public const string TruckCode_RPropertyName = "TRUCKCODE_R";
        public const string ClientOSVersionPropertyName = "CLIENTOSVERSION";
        public const string CustomParamValPropertyName = "CUSTOMPARAMVAL";
        #endregion .  Constants  .

        #region .  Properties  .
        public string ClientCode
        {
            get { return GetProperty<string>(ClientCodePropertyName); }
            set { SetProperty(ClientCodePropertyName, value); }
        }

        public string ClientName
        {
            get { return GetProperty<string>(ClientNamePropertyName); }
            set { SetProperty(ClientNamePropertyName, value); }
        }

        public string ClientMac
        {
            get { return GetProperty<string>(ClientMacPropertyName); }
            set { SetProperty(ClientMacPropertyName, value); }
        }

        public string ClientIp
        {
            get { return GetProperty<string>(ClientIpPropertyName); }
            set { SetProperty(ClientIpPropertyName, value); }
        }

        public string TruckCode_R
        {
            get { return GetProperty<string>(TruckCode_RPropertyName); }
            set { SetProperty(TruckCode_RPropertyName, value); }
        }

        public string ClientSessionOSVersion
        {
            get { return GetProperty<string>(ClientOSVersionPropertyName); }
            set { SetProperty(ClientOSVersionPropertyName, value); }
        }

        public WMSBusinessCollection<ClientCpv> CustomParamVal
        {
            get { return GetProperty<WMSBusinessCollection<ClientCpv>>(CustomParamValPropertyName); }
            set { SetProperty(CustomParamValPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}