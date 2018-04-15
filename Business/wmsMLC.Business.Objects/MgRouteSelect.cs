using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class MgRouteSelect : WMSBusinessObject
    {
        public const string GCDAYOFWEEKLPropertyName = "GCDAYOFWEEKL";
        public const string GCPARTNERGROUPLPropertyName = "GCPARTNERGROUPL";
        public const string GCPARTNERLPropertyName = "GCPARTNERL";

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCDAYOFWEEKL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCDAYOFWEEKLPropertyName); }
            set { SetProperty(GCDAYOFWEEKLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPLPropertyName); }
            set { SetProperty(GCPARTNERGROUPLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Partner> GCPARTNERL
        {
            get { return GetProperty<WMSBusinessCollection<Partner>>(GCPARTNERLPropertyName); }
            set { SetProperty(GCPARTNERLPropertyName, value); }
        }
    }
}