using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    [SourceName("CustomParamValue")]
    public class IWBCpv : CustomParamValue
    {
        #region .  Constants  .
        public new const string CustomParamCodePropertyName = "CUSTOMPARAMCODE_R_IWBCPV";
        public new const string CPV2EntityPropertyName = "CPV2ENTITY_IWBCPV";
        public new const string CPVKeyPropertyName = "CPVKEY_IWBCPV";
        public new const string CPVValuePropertyName = "CPVVALUE_IWBCPV";
        public new const string CPVParentPropertyName = "CPVPARENT_IWBCPV";
        #endregion .  Constants  .
    }
}