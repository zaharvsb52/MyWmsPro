using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class ExpiryDate : WMSBusinessObject
    {
        public const string GCARTLPropertyName = "GCARTL";
        public const string GCARTGROUPLPropertyName = "GCARTGROUPL";
        public const string GCPARTNERLPropertyName = "GCPARTNERL";
        public const string GCPARTNERGROUPLPropertyName = "GCPARTNERGROUPL";
        public const string ExpiryDateValuePropertyName = "EXPIRYDATEVALUE";
        public const string ExpiryDateValueTypePropertyName = "EXPIRYDATEVALUETYPE";
        public const string ExpiryDateUsingOptionPropertyName = "EXPIRYDATEUSINGOPTION";

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCARTL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCARTLPropertyName); }
            set { SetProperty(GCARTLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCARTGROUPL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCARTGROUPLPropertyName); }
            set { SetProperty(GCARTGROUPLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERLPropertyName); }
            set { SetProperty(GCPARTNERLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPLPropertyName); }
            set { SetProperty(GCPARTNERGROUPLPropertyName, value); }
        }

        public decimal? ExpiryDateValue
        {
            get { return GetProperty<decimal?>(ExpiryDateValuePropertyName); }
            set { SetProperty(ExpiryDateValuePropertyName, value); }
        }
        public string ExpiryDateValueType
        {
            get { return GetProperty<string>(ExpiryDateValueTypePropertyName); }
            set { SetProperty(ExpiryDateValueTypePropertyName, value); }
        }
        public string ExpiryDateUsingOption
        {
            get { return GetProperty<string>(ExpiryDateUsingOptionPropertyName); }
            set { SetProperty(ExpiryDateUsingOptionPropertyName, value); }
        }

    }
}