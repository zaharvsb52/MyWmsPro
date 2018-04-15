using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class VehiclePassTest : BaseEntityTest<VehiclePass>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "VehiclePassNumber";
        }

        protected override void FillRequiredFields(VehiclePass entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.VehicleID_r = VehicleTest.ExistsItem1Id;
            obj.VehiclePassType = TestString;
            obj.VehiclePassSeries = TestString;
            obj.CountryCode_r = IsoCountryTest.ExistsItem1Code;
        }
    }
}