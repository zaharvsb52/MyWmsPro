using System;

namespace wmsMLC.Business.Objects
{
    public class BillWorkAct : WMSBusinessObject
    {
        #region . Constants .

        public const string CONTRACTID_RPropertyName = "CONTRACTID_R";
        public const string WORKACTDATEPropertyName = "WORKACTDATE";
        public const string WORKACTDATEFROMPropertyName = "WORKACTDATEFROM";
        public const string WORKACTDATETILLPropertyName = "WORKACTDATETILL";
        public const string WORKACTDETAILEXLPropertyName = "WORKACTDETAILEXL";
        public const string WORKACTFIXDATEPropertyName = "WORKACTFIXDATE";
        public const string WORKACTHOSTREFPropertyName = "WORKACTHOSTREF";
        public const string WORKACTIDPropertyName = "WORKACTID";
        public const string WORKACTPOSTINGDATEPropertyName = "WORKACTPOSTINGDATE";
        public const string WORKACTTOTALAMOUNTPropertyName = "WORKACTTOTALAMOUNT";
        public const string WORKACTPOSTINGNUMBERPropertyName = "WORKACTPOSTINGNUMBER";
        public const string STATUSCODE_RPropertyName = "STATUSCODE_R";
        public const string WORKACT2OP2CLPropertyName = "WORKACT2OP2CL";

        public WMSBusinessCollection<BillWorkActDetailEx> WorkActDetailExL
        {
            get { return GetProperty<WMSBusinessCollection<BillWorkActDetailEx>>(WORKACTDETAILEXLPropertyName); }
            set { SetProperty(WORKACTDETAILEXLPropertyName, value); }
        }

        public WMSBusinessCollection<BillWorkAct2Op2C> WorkAct2Op2CL
        {
            get { return GetProperty<WMSBusinessCollection<BillWorkAct2Op2C>>(WORKACT2OP2CLPropertyName); }
            set { SetProperty(WORKACT2OP2CLPropertyName, value); }
        }

        public string WORKACTHOSTREF
        {
            get { return GetProperty<string>(WORKACTHOSTREFPropertyName); }
            set { SetProperty(WORKACTHOSTREFPropertyName, value); }
        }

        public DateTime? WORKACTFIXDATE
        {
            get { return GetProperty<DateTime?>(WORKACTFIXDATEPropertyName); }
            set { SetProperty(WORKACTFIXDATEPropertyName, value); }
        }

        public decimal? CONTRACTID_R
        {
            get { return GetProperty<decimal?>(CONTRACTID_RPropertyName); }
            set { SetProperty(CONTRACTID_RPropertyName, value); }
        }

        public string WORKACTPOSTINGNUMBER
        {
            get { return GetProperty<string>(WORKACTPOSTINGNUMBERPropertyName); }
            set { SetProperty(WORKACTPOSTINGNUMBERPropertyName, value); }
        }

        public string STATUSCODE_R
        {
            get { return GetProperty<string>(STATUSCODE_RPropertyName); }
            set { SetProperty(STATUSCODE_RPropertyName, value); }
        }

        public DateTime? WORKACTPOSTINGDATE
        {
            get { return GetProperty<DateTime?>(WORKACTPOSTINGDATEPropertyName); }
            set { SetProperty(WORKACTPOSTINGDATEPropertyName, value); }
        }

        public DateTime? WORKACTDATE
        {
            get { return GetProperty<DateTime?>(WORKACTDATEPropertyName); }
            set { SetProperty(WORKACTDATEPropertyName, value); }
        }


        #endregion
    }
}