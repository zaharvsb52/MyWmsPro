using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class UserGroupTest : BaseWMSObjectTest<UserGroup>
    {
        protected override void FillRequiredFields(UserGroup obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().USERGROUPCODE = TestString;
            obj.AsDynamic().USERGROUPNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERGROUPCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(UserGroup obj)
        {
            obj.AsDynamic().USERGROUPDESC = TestString;
        }

        protected override void CheckSimpleChange(UserGroup source, UserGroup dest)
        {
            string sourceName = source.AsDynamic().UserGroupName;
            string destName = dest.AsDynamic().UserGroupName;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}