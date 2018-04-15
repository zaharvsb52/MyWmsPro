using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RightTest : BaseWMSObjectTest<Right>
    {
        protected override void FillRequiredFields(Right obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().RIGHTCODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RIGHTCODE = '{0}')", TestString);
        }
        
        protected override void MakeSimpleChange(Right obj)
        {
            obj.AsDynamic().RIGHTNAME = TestString;
        }

        protected override void CheckSimpleChange(Right source, Right dest)
        {
            string sourceName = source.AsDynamic().RightName;
            string destName = dest.AsDynamic().RightName;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
