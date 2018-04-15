using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TruckTest : BaseEntityTest<Truck>
    {
        public const string ExistsItem1Code = "TST_TRUCK_1";
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TruckDesc";
        }

        protected override void FillRequiredFields(Truck entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TruckTypeCode_r = TruckTypeTest.ExistsItem1Code;
            obj.TruckName = TestString;
        }
    }
}