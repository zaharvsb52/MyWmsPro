namespace wmsMLC.Business.Objects
{
    public class EcomClient : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ClientlastnamePropertyName = "CLIENTLASTNAME";
        public const string ClientmiddlenamePropertyName = "CLIENTMIDDLENAME";
        public const string ClientnamePropertyName = "CLIENTNAME";
        public const string ClienthostrefPropertyName = "CLIENTHOSTREF";
        public const string AddresslPropertyName = "ADDRESSL";
        public const string ClientIDPropertyName = "CLIENTID";
        public const string MandantIDPropertyName = "MANDANTID";
        #endregion

        #region .  Properties  .
        public WMSBusinessCollection<AddressBook> Address
        {
            get { return GetProperty<WMSBusinessCollection<AddressBook>>(AddresslPropertyName); }
            set { SetProperty(AddresslPropertyName, value); }
        }
        public decimal ClientID
        {
            get { return GetProperty<decimal>(ClientIDPropertyName); }
            set { SetProperty(ClientIDPropertyName, value); }
        }

        public decimal MandantID
        {
            get { return GetProperty<decimal>(MandantIDPropertyName); }
            set { SetProperty(MandantIDPropertyName, value); }
        }
        #endregion

    }
}