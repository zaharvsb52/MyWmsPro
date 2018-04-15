using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PMTest : BaseWMSObjectTest<PM>
    {
        protected override void FillRequiredFields(PM obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PMCODE = TestString;
            obj.AsDynamic().PMNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PMCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(PM obj)
        {
            obj.AsDynamic().PMDESC = TestString;
        }

        protected override void CheckSimpleChange(PM source, PM dest)
        {
            string sourceName = source.AsDynamic().PMDESC;
            string destName = dest.AsDynamic().PMDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}