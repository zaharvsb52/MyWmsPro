using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class MgRouteDateSelect : WMSBusinessObject
    {
        public const string GCPARTNERGROUPLPropertyName = "GCPARTNERGROUPL";
        public const string GCPARTNERLPropertyName = "GCPARTNERL";

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPLPropertyName); }
            set { SetProperty(GCPARTNERGROUPLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERLPropertyName); }
            set { SetProperty(GCPARTNERLPropertyName, value); }
        }
    }
}