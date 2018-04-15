using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillStrategyUseValuesTest : BaseEntityTest<BillStrategyUseValues>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "STRATEGYUSEVALUESVALUE";
        }

        protected override void FillRequiredFields(BillStrategyUseValues entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.STRATEGYUSEID_R = BillStrategyUseTest.ExistsItem1Id;
            obj.STRATEGYPARAMSID_R = BillStrategyParamsTest.ExistsItem1Id;
            obj.STRATEGYUSEVALUESVALUE = TestString;
        }
    }
}