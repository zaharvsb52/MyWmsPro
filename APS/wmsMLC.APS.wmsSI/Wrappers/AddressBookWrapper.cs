namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class AddressBookWrapper : BaseWrapper
    {
        #region . Properties .
        public string ADDRESSBOOKAPARTMENT { get; set; }
        public string ADDRESSBOOKBUILDING { get; set; }
        public string ADDRESSBOOKCITY { get; set; }
        public string ADDRESSBOOKCOUNTRY { get; set; }
        public string ADDRESSBOOKDISTRICT { get; set; }
        public decimal? ADDRESSBOOKID { get; set; }
        public string ADDRESSBOOKINDEXSTR { get; set; }
        public string ADDRESSBOOKREGION { get; set; }
        public string ADDRESSBOOKSTREET { get; set; }
        public AddressBookType ADDRESSBOOKTYPECODE { get; set; }
        public string ADDRESSBOOKRAW { get; set; }
        public string ADDRESSBOOKHOSTREF { get; set; }

        #endregion . Properties .
    }

    public enum AddressBookType
    {
        ADR_PHYSICAL, ADR_LEGAL, ADR_POST, ADR_UNLOADING_POINT, ADR_CLIENTRECIPIENT
    }
}
