using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PrinterLogicalTest : BaseEntityTest<PrinterLogical>
    {
        public const string ExistsItem1Code = "TST_PRINTERLOGICAL_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "LOGICALPRINTERDESC";
        }

        protected override void FillRequiredFields(PrinterLogical entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PhysicalPrinter_r = PrinterPhysicalTest.ExistsItem1Code;
            obj.LogicalPrinterCopies = TestDecimal;
            obj.LogicalPrinterLocked = TestBool;
            obj.LogicalPrinterTray = TestDecimal;
        }
    }
}