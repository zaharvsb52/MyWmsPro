using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BarcodeTest : BaseEntityTest<Barcode>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Barcode.BarcodeValuePropertyName;
        }

        protected override void FillRequiredFields(Barcode entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BARCODEKEY = TestString;
            obj.BARCODEVALUE = TestString;
            obj.BARCODE2ENTITY = "BARCODE";
        }
    }
}