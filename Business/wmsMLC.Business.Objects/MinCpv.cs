using wmsMLC.General;

namespace wmsMLC.Business.Objects
{
    [SourceName("CustomParamValue")]
    public class MinCpv : CustomParamValue
    {
        public const string MINOverEnableCpvName = "MINOverEnableL3";
        public const string MINOverEnableNeedConfirmCpvName = "MINOverEnableNeedConfirmL4";
        public const string MinLimitL2CpvName = "MINLimitL2";
        public const string MinExpiryDateInLimitAsk = "MINExpiryDateInLimitAsk";
        public const string MinExpiryDateInLimitDeny = "MINExpiryDateInLimitDeny";
        public const string MinExpiryDateInLimitAskBlock = "MINExpiryDateInLimitAskBlock";
        public const string DefaultBlock = "DefaultBlock";
        public const string CanOnDclBlock = "CanOnDclBlock";
        public const string MinExpiryDateInLimitAskQlf = "MINExpiryDateInLimitAskQLF";
        public const string DefaultQlf = "DefaultQlf";
        public const string CanOnDclQlf = "CanOnDclQlf";
    }
}