using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillStrategyParamsTest : BaseEntityTest<BillStrategyParams>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "STRATEGYPARAMSNAME";
        }

        protected override void FillRequiredFields(BillStrategyParams entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.STRATEGYCODE_R = BillStrategyTest.ExistsItem1Code;
            obj.STRATEGYPARAMSNAME = TestString;
            obj.STRATEGYPARAMSDATATYPE = TestDecimal;
            obj.STRATEGYPARAMSINDEX = TestDecimal;
        }
    }
}