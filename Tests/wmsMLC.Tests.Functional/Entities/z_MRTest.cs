using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MRTest : BaseWMSObjectTest<MR>
    {
        protected override void FillRequiredFields(MR obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MRCODE = TestString;
            obj.AsDynamic().MRNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MRCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MR obj)
        {
            obj.AsDynamic().MRDESC = TestString;
        }

        protected override void CheckSimpleChange(MR source, MR dest)
        {
            string sourceName = source.AsDynamic().MRDESC;
            string destName = dest.AsDynamic().MRDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}