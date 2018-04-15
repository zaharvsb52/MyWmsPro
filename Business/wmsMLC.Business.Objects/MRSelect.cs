using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class MRSelect : WMSBusinessObject
    {
        #region .  Constants  .

        public const string GCPARTNERGROUPLPropertyName = "GCPARTNERGROUPL";
        public const string GCARTGROUPLPropertyName = "GCARTGROUPL";
        public const string GCARTGROUPDANGERLPropertyName = "GCARTGROUPDANGERL";
        public const string GCQLFLPropertyName = "GCQLFL";
       
        #endregion .  Constants  .


        #region .  Properties  .

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCPARTNERGROUPL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCPARTNERGROUPLPropertyName); }
            set { SetProperty(GCPARTNERGROUPLPropertyName, value); }
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
        public WMSBusinessCollection<Entity2GC> GCARTGROUPDANGERL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCARTGROUPDANGERLPropertyName); }
            set { SetProperty(GCARTGROUPDANGERLPropertyName, value); }
        }

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCQLFL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCQLFLPropertyName); }
            set { SetProperty(GCQLFLPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}