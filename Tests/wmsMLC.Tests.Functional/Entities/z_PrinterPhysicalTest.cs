using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PrinterPhysicalTest : BaseWMSObjectTest<PrinterPhysical>
    {
        public const bool TestBool = false;

        protected override string GetCheckFilter()
        {
            return string.Format("(PHYSICALPRINTER = '{0}')", TestString);
        }

        protected override void FillRequiredFields(PrinterPhysical obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PHYSICALPRINTER = TestString;
            obj.AsDynamic().PHYSICALPRINTERLOCKED = TestBool;
        }

        protected override void MakeSimpleChange(PrinterPhysical obj)
        {
            obj.AsDynamic().PHYSICALPRINTERDESC = TestString + "002";
        }

        protected override void CheckSimpleChange(PrinterPhysical source, PrinterPhysical dest)
        {
            string sourceName = source.AsDynamic().PHYSICALPRINTERDESC;
            string destName = dest.AsDynamic().PHYSICALPRINTERDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}