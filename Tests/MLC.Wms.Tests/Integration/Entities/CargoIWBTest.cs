using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CargoIWBTest : BaseEntityTest<CargoIWB>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = CargoIWB.CargoIWBNetPropertyName;
            InsertItemTransact = 2;
            UpdateItemTransact = 4;
        }

        protected override void FillRequiredFields(CargoIWB entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CARGOIWBNET = TestDecimal;
        }
    }
}