using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CargoIWBPosTest : BaseEntityTest<CargoIWBPos>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = CargoIWBPos.CARGOIWBPOSDESCPropertyName;
        }

        protected override void FillRequiredFields(CargoIWBPos entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CARGOIWBID_R = CargoIWBTest.ExistsItem1Code;
            obj.CARGOIWBPOSCOUNT = TestDecimal;
            obj.TETYPECODE_R = TETypeTest.ExistsItem1Code;
            obj.CARGOIWBPOSTYPE = TestString;
            obj.QLFCODE_R = QlfTest.ExistsItem1Code;
        }
    }
}