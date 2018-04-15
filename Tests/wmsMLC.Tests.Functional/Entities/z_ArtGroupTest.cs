using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ArtGroupTest : BaseWMSObjectTest<ArtGroup>
    {
        protected override void FillRequiredFields(ArtGroup obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ARTGROUPCODE = TestString;
            obj.AsDynamic().ARTGROUPNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ARTGROUPCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(ArtGroup obj)
        {
            obj.AsDynamic().ARTGROUPDESC = TestString;
        }

        protected override void CheckSimpleChange(ArtGroup source, ArtGroup dest)
        {
            string sourceName = source.AsDynamic().ARTGROUPDESC;
            string destName = dest.AsDynamic().ARTGROUPDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}