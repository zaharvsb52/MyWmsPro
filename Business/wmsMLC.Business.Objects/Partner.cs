using System.Collections.Generic;
using System.Linq;

namespace wmsMLC.Business.Objects
{
    public class Partner : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ADDRESSPropertyName = "ADDRESS";
        public const string GLOBALPARAMVALPropertyName = "GLOBALPARAMVAL";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string PARTNERBIKPropertyName = "PARTNERBIK";
        public const string PARTNERCODEPropertyName = "PARTNERCODE";
        public const string PARTNERCONTRACTPropertyName = "PARTNERCONTRACT";
        public const string PARTNERCORRESPONDENTACCOUNTPropertyName = "PARTNERCORRESPONDENTACCOUNT";
        public const string PARTNERDATECONTRACTPropertyName = "PARTNERDATECONTRACT";
        public const string PARTNEREMAILPropertyName = "PARTNEREMAIL";
        public const string PARTNERFAXPropertyName = "PARTNERFAX";
        public const string PARTNERFULLNAMEPropertyName = "PARTNERFULLNAME";
        public const string PARTNERHOSTREFPropertyName = "PARTNERHOSTREF";
        public const string PARTNERIDPropertyName = "PARTNERID";
        public const string PARTNERINNPropertyName = "PARTNERINN";
        public const string PARTNERKPPPropertyName = "PARTNERKPP";
        public const string PARTNERLOCKEDPropertyName = "PARTNERLOCKED";
        public const string PARTNERNAMEPropertyName = "PARTNERNAME";
        public const string PARTNEROGRNPropertyName = "PARTNEROGRN";
        public const string PARTNEROKPOPropertyName = "PARTNEROKPO";
        public const string PARTNEROKVEDPropertyName = "PARTNEROKVED";
        public const string PARTNERPHONEPropertyName = "PARTNERPHONE";
        public const string PARTNERSETTLEMENTACCOUNTPropertyName = "PARTNERSETTLEMENTACCOUNT";
        public const string PARTNERLINK2MANDANTPropertyName = "PARTNERLINK2MANDANT";
        public const string PARTNERCOMMERCTIMEPropertyName = "PARTNERCOMMERCTIME";
        public const string PARTNERCOMMERCTIMEMEASUREPropertyName = "PARTNERCOMMERCTIMEMEASURE";
        public const string VADDRESSBOOKCOMPLEXPropertyName = "VADDRESSBOOKCOMPLEX";
        public const string EMPLOYEELPropertyName = "EMPLOYEEL";
        #endregion .  Constants  .

        #region .  Properties  .
        /// <summary>
        /// Код манданта (владельца).
        /// </summary>
        public decimal? MandantId
        {
            get { return GetProperty<decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        public decimal? PartnerId
        {
            get { return GetProperty<decimal?>(PARTNERIDPropertyName); }
            set { SetProperty(PARTNERIDPropertyName, value); }
        }

        public WMSBusinessCollection<AddressBook> Address
        {
            get { return GetProperty<WMSBusinessCollection<AddressBook>>(ADDRESSPropertyName); }
            set { SetProperty(ADDRESSPropertyName, value); }
        }

        public WMSBusinessCollection<PartnerGpv> GlobalParamVal
        {
            get { return GetProperty<WMSBusinessCollection<PartnerGpv>>(GLOBALPARAMVALPropertyName); }
            set { SetProperty(GLOBALPARAMVALPropertyName, value); }
        }

        public string PartnerName
        {
            get { return GetProperty<string>(PARTNERNAMEPropertyName); }
            set { SetProperty(PARTNERNAMEPropertyName, value); }
        }
        
        public string PartnerCode
        {
            get { return GetProperty<string>(PARTNERCODEPropertyName); }
            set { SetProperty(PARTNERCODEPropertyName, value); }
        }

        public string PartnerFullName
        {
            get { return GetProperty<string>(PARTNERFULLNAMEPropertyName); }
            set { SetProperty(PARTNERFULLNAMEPropertyName, value); }
        }

        public string PartnerHostRef
        {
            get { return GetProperty<string>(PARTNERHOSTREFPropertyName); }
            set { SetProperty(PARTNERHOSTREFPropertyName, value); }
        }

        public string PartnerINN
        {
            get { return GetProperty<string>(PARTNERINNPropertyName); }
            set { SetProperty(PARTNERINNPropertyName, value); }
        }

        public string PartnerOKPO
        {
            get { return GetProperty<string>(PARTNEROKPOPropertyName); }
            set { SetProperty(PARTNEROKPOPropertyName, value); }
        }

        public string PartnerPhone
        {
            get { return GetProperty<string>(PARTNERPHONEPropertyName); }
            set { SetProperty(PARTNERPHONEPropertyName, value); }
        }

        public string PartnerCommercTimeMeasure
        {
            get { return GetProperty<string>(PARTNERCOMMERCTIMEMEASUREPropertyName); }
            set { SetProperty(PARTNERCOMMERCTIMEMEASUREPropertyName, value); }
        }

        public decimal? PartnerCommercTime
        {
            get { return GetProperty<decimal?>(PARTNERCOMMERCTIMEPropertyName); }
            set { SetProperty(PARTNERCOMMERCTIMEPropertyName, value); }
        }

        public WMSBusinessCollection<Employee> EmployeeL
        {
            get { return GetProperty<WMSBusinessCollection<Employee>>(EMPLOYEELPropertyName); }
            set { SetProperty(EMPLOYEELPropertyName, value); }
        }

        #endregion .  Properties  .

        #region .  Methods  .
        public static IEnumerable<Partner> SplitWithAddress(IEnumerable<Partner> source)
        {
            var result = new List<Partner>();
            foreach (var s in source)
            {
                if (s.Address == null || !s.Address.Any())
                {
                    result.Add(s);
                    continue;
                }
                foreach (var a in s.Address)
                {
                    var p = (Partner)s.Clone();
                    p.SetProperty(Partner.VADDRESSBOOKCOMPLEXPropertyName, a);
                    p.Address = new WMSBusinessCollection<AddressBook>(new[] { a });
                    result.Add(p);
                }
            }
            return result;
        }
        #endregion
    }
}