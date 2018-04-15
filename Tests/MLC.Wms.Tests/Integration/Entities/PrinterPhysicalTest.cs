using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PrinterPhysicalTest : BaseEntityTest<PrinterPhysical>
    {
        public const string ExistsItem1Code = "TST_PRINTERPHYSICAL_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PHYSICALPRINTERDESC";
        }

        protected override void FillRequiredFields(PrinterPhysical entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PhysicalPrinterLocked = TestBool;
        }
    }
}