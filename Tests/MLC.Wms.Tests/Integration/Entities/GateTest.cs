using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class GateTest : BaseEntityTest<Gate>
    {
        public const string ExistsItem1Code = "TST_GATE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "GATENAME";
        }

        protected override void FillRequiredFields(Gate entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WAREHOUSECODE_R = WarehouseTest.ExistsItem1Code;
            obj.GATENAME = TestString;
            obj.GATENUMBER = TestString;
        }
    }
}