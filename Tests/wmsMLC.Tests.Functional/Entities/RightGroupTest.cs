using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RightGroupTest : BaseWMSObjectTest<RightGroup>
    {
        protected override void FillRequiredFields(RightGroup obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().RIGHTGROUPCODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RIGHTGROUPCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(RightGroup obj)
        {
            obj.AsDynamic().RIGHTGROUPDESC = TestString;
        }

        protected override void CheckSimpleChange(RightGroup source, RightGroup dest)
        {
            string sourceName = source.AsDynamic().RIGHTGROUPDESC;
            string destName = dest.AsDynamic().RIGHTGROUPDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test,Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}