using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillStrategyTest : BaseEntityTest<BillStrategy>
    {
        public const string ExistsItem1Code = "TST_BILLSTRATEGY_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "STRATEGYNAME";
        }

        protected override void FillRequiredFields(BillStrategy entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.STRATEGYNAME = TestString;
            obj.STRATEGYGROUP = TestString;
        }
    }
}