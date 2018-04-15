using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillScaleTest : BaseEntityTest<BillScale>
    {
        public const string ExistsItem1Code = "TST_BILLSCALE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SCALENAME";
        }
    }
}