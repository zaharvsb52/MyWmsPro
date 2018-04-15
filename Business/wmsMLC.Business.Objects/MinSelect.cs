using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class MinSelect : WMSBusinessObject
    {
        public const string GCPARTNERGROUPRECIPIENTLPropertyName = "GCPARTNERGROUPRECIPIENTL";
        public const string GCPARTNERGROUPCARRIERLPropertyName = "GCPARTNERGROUPCARRIERL";
        public const string GCPARTNERGROUPSENDERLPropertyName = "GCPARTNERGROUPSENDERL";
        public const string GCPARTNERSENDERLPropertyName = "GCPARTNERSENDERL";
        

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPRECIPIENTL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPRECIPIENTLPropertyName); }
            set { SetProperty(GCPARTNERGROUPRECIPIENTLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPCARRIERL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPCARRIERLPropertyName); }
            set { SetProperty(GCPARTNERGROUPCARRIERLPropertyName, value); }
        }
        
        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPSENDERL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPSENDERLPropertyName); }
            set { SetProperty(GCPARTNERGROUPSENDERLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERSENDERL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERSENDERLPropertyName); }
            set { SetProperty(GCPARTNERSENDERLPropertyName, value); }
        }
    }
}