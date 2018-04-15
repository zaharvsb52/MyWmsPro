using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class ObjectConfig : WMSBusinessObject
    {
        public const string GCEVENTKINDLPropertyName = "GCEVENTKINDL";
        
        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCEVENTKINDL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCEVENTKINDLPropertyName); }
            set { SetProperty(GCEVENTKINDLPropertyName, value); }
        }
    }
}