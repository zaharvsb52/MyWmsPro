using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WarehouseTest : BaseEntityTest<Warehouse>
    {
        public const string ExistsItem1Code = "TST_WAREHOUSE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WAREHOUSEDESC";
        }

        protected override void FillRequiredFields(Warehouse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.AsDynamic().WAREHOUSENAME = TestString;
        }
    }
}