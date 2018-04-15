using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PMMethodTest : BaseWMSObjectTest<PMMethod>
    {
        protected override void FillRequiredFields(PMMethod obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PMMETHODCODE = TestString;
            obj.AsDynamic().PMMETHODNAME = TestString;
            obj.AsDynamic().PMMETHODDESC = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PMMETHODCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(PMMethod obj)
        {
            obj.AsDynamic().PMMETHODDESC = TestString;
        }

        protected override void CheckSimpleChange(PMMethod source, PMMethod dest)
        {
            string sourceDesc = source.AsDynamic().PMMETHODDESC;
            string destDesc = dest.AsDynamic().PMMETHODDESC;
            sourceDesc.ShouldBeEquivalentTo(destDesc);
        }
    }
}