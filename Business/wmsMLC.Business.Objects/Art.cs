using System;

namespace wmsMLC.Business.Objects
{
    public class Art : WMSBusinessObject
    {
        #region .  Constants  .
        public const string ArtNamePropertyName = "ARTNAME";
        public const string ArtDescPropertyName = "ARTDESC";
        public const string ArtInputDateMethodPropertyName = "ARTINPUTDATEMETHOD";
        public const string ARTABCDPropertyName = "ARTABCD";
        public const string ARTCODEPropertyName = "ARTCODE";
        public const string ARTCOLORPropertyName = "ARTCOLOR";
        public const string ARTCOLORTONEPropertyName = "ARTCOLORTONE";
        public const string ARTCOMMERCDAYPropertyName = "ARTCOMMERCDAY";
        public const string ARTDESCPropertyName = "ARTDESC";
        public const string ARTDESCEXTPropertyName = "ARTDESCEXT";
        public const string FACTORYID_RPropertyName = "FACTORYID_R";
        public const string ARTHOSTREFPropertyName = "ARTHOSTREF";
        public const string ARTINPUTDATEMETHODPropertyName = "ARTINPUTDATEMETHOD";
        public const string ARTPICKORDERPropertyName = "ARTPICKORDER";
        public const string ARTSIZEPropertyName = "ARTSIZE";
        public const string ARTTEMPMAXPropertyName = "ARTTEMPMAX";
        public const string ARTTEMPMINPropertyName = "ARTTEMPMIN";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";
        public const string GROUP2ARTLPropertyName = "GROUP2ARTL";
        public const string MANDANTIDPropertyName = "MANDANTID";
        public const string SKULPropertyName = "SKUL";
        public const string ARTDANGERLEVELPropertyName = "ARTDANGERLEVEL";
        public const string ARTDANGERSUBLEVELPropertyName = "ARTDANGERSUBLEVEL";
        public const string ARTLIFETIMEPropertyName = "ARTLIFETIME";
        public const string ARTLIFETIMEMEASUREPropertyName = "ARTLIFETIMEMEASURE";
        public const string ARTIWBTYPEPropertyName = "ARTIWBTYPE";
        public const string ARTCOMMERCTIMEMEASUREPropertyName = "ARTCOMMERCTIMEMEASURE";
        public const string ARTCOMMERCTIMEPropertyName = "ARTCOMMERCTIME";
        public const string ARTBRANDPropertyName = "ARTBRAND";
        public const string ARTMARKPropertyName = "ARTMARK";
        public const string ARTMODELPropertyName = "ARTMODEL";
        public const string ARTTYPEPropertyName = "ARTTYPE";
        public const string ARTPICTUREPropertyName = "ARTPICTURE";
        #endregion .  Constants  .

        #region .  Properties  .
        public string ArtCode
        {
            get { return GetProperty<string>(ARTCODEPropertyName); }
            set { SetProperty(ARTCODEPropertyName, value); }
        }

        public string ArtName
        {
            get { return GetProperty<string>(ArtNamePropertyName); }
            set { SetProperty(ArtNamePropertyName, value); }
        }

        public string ArtDesc
        {
            get { return GetProperty<string>(ArtDescPropertyName); }
            set { SetProperty(ArtDescPropertyName, value); }
        }

        public string ArtInputDateMethod
        {
            get { return GetProperty<string>(ArtInputDateMethodPropertyName); }
            set { SetProperty(ArtInputDateMethodPropertyName, value); }
        }

        public Decimal? MANDANTID
        {
            get { return GetProperty<Decimal?>(MANDANTIDPropertyName); }
            set { SetProperty(MANDANTIDPropertyName, value); }
        }

        public string ARTABCD
        {
            get { return GetProperty<string>(ARTABCDPropertyName); }
            set { SetProperty(ARTABCDPropertyName, value); }
        }

        public decimal ArtPickOrder
        {
            get { return GetProperty<decimal>(ARTPICKORDERPropertyName); }
            set { SetProperty(ARTPICKORDERPropertyName, value); }
        }

        public decimal ArtCommercDay
        {
            get { return GetProperty<decimal>(ARTCOMMERCDAYPropertyName); }
            set { SetProperty(ARTCOMMERCDAYPropertyName, value); }
        }

        public decimal? ArtCommercTime
        {
            get { return GetProperty<decimal?>(ARTCOMMERCTIMEPropertyName); }
            set { SetProperty(ARTCOMMERCTIMEPropertyName, value); }
        }

        public string ArtCommercTimeMeasure
        {
            get { return GetProperty<string>(ARTCOMMERCTIMEMEASUREPropertyName); }
            set { SetProperty(ARTCOMMERCTIMEMEASUREPropertyName, value); }
        }

        public decimal ArtLifeTime
        {
            get { return GetProperty<decimal>(ARTLIFETIMEPropertyName); }
            set { SetProperty(ARTLIFETIMEPropertyName, value); }
        }
        public string ArtLifeTimeMeasure
        {
            get { return GetProperty<string>(ARTLIFETIMEMEASUREPropertyName); }
            set { SetProperty(ARTLIFETIMEMEASUREPropertyName, value); }
        }

        public decimal? FactoryID_R
        {
            get { return GetProperty<decimal?>(FACTORYID_RPropertyName); }
            set { SetProperty(FACTORYID_RPropertyName, value); }
        }

        public double? ArtTempMax
        {
            get { return GetProperty<double?>(ARTTEMPMAXPropertyName); }
            set { SetProperty(ARTTEMPMAXPropertyName, value); }
        }

        public double? ArtTempMin
        {
            get { return GetProperty<double?>(ARTTEMPMINPropertyName); }
            set { SetProperty(ARTTEMPMINPropertyName, value); }
        }

        public string ArtHostRef
        {
            get { return GetProperty<string>(ARTHOSTREFPropertyName); }
            set { SetProperty(ARTHOSTREFPropertyName, value); }
        }

        public WMSBusinessCollection<SKU> SKUL
        {
            get { return GetProperty<WMSBusinessCollection<SKU>>(SKULPropertyName); }
            set { SetProperty(SKULPropertyName, value); }
        }

        public WMSBusinessCollection<Art2Group> Art2GroupL
        {
            get { return GetProperty<WMSBusinessCollection<Art2Group>>(GROUP2ARTLPropertyName); }
            set { SetProperty(GROUP2ARTLPropertyName, value); }
        }

        public WMSBusinessCollection<ArtCpv> CUSTOMPARAMVAL
        {
            get { return GetProperty<WMSBusinessCollection<ArtCpv>>(CUSTOMPARAMVALPropertyName); }
            set { SetProperty(CUSTOMPARAMVALPropertyName, value); }
        }
        public decimal? ARTPICTURE
        {
            get { return GetProperty<decimal?>(ARTPICTUREPropertyName); }
            set { SetProperty(ARTPICTUREPropertyName, value); }
        }
        #endregion .  Properties  .
    }
}