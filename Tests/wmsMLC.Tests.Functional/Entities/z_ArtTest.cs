using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ArtTest : BaseWMSObjectTest<Art>
    {
        protected override void FillRequiredFields(Art obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ARTNAME = TestString;
            obj.AsDynamic().MANDANTID = 1; 
            obj.AsDynamic().ARTABCD = "A";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ARTNAME = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Art obj)
        {
            obj.AsDynamic().ARTDESC = TestString;
        }

        protected override void CheckSimpleChange(Art source, Art dest)
        {
            string sourceName = source.AsDynamic().ARTDESC;
            string destName = dest.AsDynamic().ARTDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

    }
}