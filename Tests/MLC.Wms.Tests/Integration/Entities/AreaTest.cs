using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class AreaTest : BaseEntityTest<Area>
    {
        public const string ExistsItem1Code = "TST_AREA_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "AREADESC";
        }

        protected override void FillRequiredFields(Area entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.AREANAME = TestString;
            obj.AREATYPECODE_R = AreaTypeTest.ExistsItem1Code;
            obj.WAREHOUSECODE_R = WarehouseTest.ExistsItem1Code;
        }
    }
}