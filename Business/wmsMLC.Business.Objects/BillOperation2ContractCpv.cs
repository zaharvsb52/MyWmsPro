using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    [SourceName("CustomParamValue")]
    public class BillOperation2ContractCpv : CustomParamValue
    {
        #region .  Constants  .
        public new const string CustomParamCodePropertyName = "BILLOPERATION2CONTRACTCPVCUSTOMPARAMCODE";
        //public const string CPV2EntityPropertyName = "CPV2ENTITY_BILLOPERATION2CONTRACTCPV";
        //public const string CPVKeyPropertyName = "CPVKEY_BILLOPERATION2CONTRACTCPV";
        //public const string CPVValuePropertyName = "CPVVALUE_BILLOPERATION2CONTRACTCPV";
        //public const string CPVParentPropertyName = "CPVPARENT_BILLOPERATION2CONTRACTCPV";
        #endregion .  Constants  .

        public override string ChangePropertyName(string basePropertyName)
        {
            return basePropertyName.EqIgnoreCase(CustomParamValue.CustomParamCodePropertyName)
                ? CustomParamCodePropertyName
                : base.ChangePropertyName(basePropertyName);
        }
    }
}