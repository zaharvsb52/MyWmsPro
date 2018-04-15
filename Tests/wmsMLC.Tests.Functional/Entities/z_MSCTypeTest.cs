using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MSCTypeTest : BaseWMSObjectTest<MSCType>
    {
        protected override void FillRequiredFields(MSCType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MSCTYPECODE = TestString;
            obj.AsDynamic().MSCTYPENAME = TestString;
            obj.AsDynamic().MSCTYPEORDER = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MSCTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MSCType obj)
        {
            obj.AsDynamic().MSCTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(MSCType source, MSCType dest)
        {
            string sourceName = source.AsDynamic().MSCTYPEDESC;
            string destName = dest.AsDynamic().MSCTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}