using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class VehicleTest : BaseEntityTest<Vehicle>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Vehicle.VehicleRNPropertyName;
        }

        protected override void FillRequiredFields(Vehicle entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.VEHICLERN = TestString;
            obj.CARTYPEID_R = CarTypeTest.ExistsItem1Code;
        }

        protected override void CheckSimpleChange(Vehicle entity, Vehicle updated)
        {
            var sourceValue = entity.GetProperty<string>(Vehicle.VehicleRNPropertyName);
            var targetValue = updated.GetProperty<string>(Vehicle.VehicleRNPropertyName);
            // VehicleRN приводим к верхнему регистру
            targetValue.Should().Be(sourceValue.ToUpper());
        }
    }
}