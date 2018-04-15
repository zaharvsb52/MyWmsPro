using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SupplyAreaTest : BaseEntityTest<SupplyArea>
    {
        public const string ExistsItem1Code = "TST_SUPPLYAREA_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SupplyAreaDesc";
        }

        protected override void FillRequiredFields(SupplyArea entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SupplyAreaName = TestString;
        }
    }
}