using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CarTypeTest : BaseEntityTest<CarType>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = CarType.CarTypeDescPropertyName;
        }

        protected override void FillRequiredFields(CarType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            // req
            obj.CARTYPEMARK = TestString;
        }
    }
}