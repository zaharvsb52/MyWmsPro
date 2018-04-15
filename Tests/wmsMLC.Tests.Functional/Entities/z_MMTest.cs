using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MMTest : BaseWMSObjectTest<MM>
    {
        protected override void FillRequiredFields(MM obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MMCODE = TestString;
            obj.AsDynamic().MMNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MMCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MM obj)
        {
            obj.AsDynamic().MMDESC = TestString;
        }

        protected override void CheckSimpleChange(MM source, MM dest)
        {
            string sourceName = source.AsDynamic().MMDESC;
            string destName = dest.AsDynamic().MMDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}