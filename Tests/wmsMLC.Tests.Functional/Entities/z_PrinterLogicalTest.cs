using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PrinterLogicalTest : BaseWMSObjectTest<PrinterLogical>
    {
        public const bool TestBool = false;
        private readonly PrinterPhysicalTest _printerPhysicalTest = new PrinterPhysicalTest();

        protected override string GetCheckFilter()
        {
            return string.Format("(LOGICALPRINTER = '{0}')", TestString);
        }

        protected override void FillRequiredFields(PrinterLogical obj)
        {
            base.FillRequiredFields(obj);

            var printerPhysical = _printerPhysicalTest.CreateNew();

            obj.AsDynamic().LOGICALPRINTER = TestString;
            obj.AsDynamic().PHYSICALPRINTER_R = printerPhysical.GetKey();
            obj.AsDynamic().LOGICALPRINTERCOPIES = TestDecimal;
            obj.AsDynamic().LOGICALPRINTERLOCKED = TestBool;
        }

        protected override void MakeSimpleChange(PrinterLogical obj)
        {
            obj.AsDynamic().LOGICALPRINTERDESC = TestString;
        }

        protected override void CheckSimpleChange(PrinterLogical source, PrinterLogical dest)
        {
            string sourceName = source.AsDynamic().LOGICALPRINTERDESC;
            string destName = dest.AsDynamic().LOGICALPRINTERDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _printerPhysicalTest.ClearForSelf();
        }
    }
}
