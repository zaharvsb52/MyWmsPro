using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ParkingTest : BaseEntityTest<Parking>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PARKINGAREA";
        }

        protected override void FillRequiredFields(Parking entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();
           
            obj.PARKINGNUMBER = TestString;
            obj.PARKINGNAME = TestString;
        }
    }
}
