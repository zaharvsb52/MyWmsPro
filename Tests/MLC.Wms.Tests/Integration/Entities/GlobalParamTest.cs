using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class GlobalParamTest : BaseEntityTest<GlobalParam>
    {
        public const string ExistsItem1Code = "TST_GLOBALPARAM_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = GlobalParam.GlobalParamDescPropertyName;
        }
    }
}