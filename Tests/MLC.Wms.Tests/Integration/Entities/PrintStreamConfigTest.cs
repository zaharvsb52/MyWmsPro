using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PrintStreamConfigTest : BaseEntityTest<PrintStreamConfig>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PRINTSTREAMDESC";
        }

        protected override void FillRequiredFields(PrintStreamConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.LogicalPrinter_r = PrinterLogicalTest.ExistsItem1Code;
            obj.PrintStreamCopies = TestDecimal;
            obj.PrintStreamLocked = TestBool;
        }
    }
}