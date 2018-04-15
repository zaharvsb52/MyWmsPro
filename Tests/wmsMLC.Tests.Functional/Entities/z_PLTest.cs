using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PLTest : BaseWMSObjectTest<MPL>
    {
        protected override void FillRequiredFields(MPL obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MPLCODE = TestString;
            obj.AsDynamic().MPLNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MPLCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MPL obj)
        {
            obj.AsDynamic().MPLDESC = TestString;
        }

        protected override void CheckSimpleChange(MPL source, MPL dest)
        {
            string sourceName = source.AsDynamic().MPLDESC;
            string destName = dest.AsDynamic().MPLDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}