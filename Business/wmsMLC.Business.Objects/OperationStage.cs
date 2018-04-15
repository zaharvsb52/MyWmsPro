using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Business.Objects
{
    public class OperationStage : WMSBusinessObject
    {

        #region .  Constants  .

        public const string GCMANDANTLPropertyName = "GCMANDANTL";

        #endregion .  Constants  .


        #region .  Properties  .

        [XmlNotIgnore]
        [GCField]
        public WMSBusinessCollection<Entity2GC> GCMANDANTL
        {
            get { return GetProperty<WMSBusinessCollection<Entity2GC>>(GCMANDANTLPropertyName); }
            set { SetProperty(GCMANDANTLPropertyName, value); }
        }

        #endregion .  Properties  .
    }
}