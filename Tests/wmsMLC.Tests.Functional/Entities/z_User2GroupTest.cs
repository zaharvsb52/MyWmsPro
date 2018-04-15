using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class User2GroupTest : BaseWMSObjectTest<User2Group>
    {
        private readonly UserTest _userTest = new UserTest();
        private readonly UserGroupTest _userGroupTest = new UserGroupTest();

        public User2GroupTest()
        {
            _userTest.TestString = TestString;
            _userGroupTest.TestString = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERGROUPCODE_R='{0}')", TestString);
        }

        protected override void FillRequiredFields(User2Group obj)
        {
            base.FillRequiredFields(obj);

            var u = _userTest.CreateNew();
            var ug = _userGroupTest.CreateNew();

            obj.AsDynamic().USER2GROUPID = TestDecimal;
            obj.AsDynamic().USER2GROUPUSERCODE = u.GetKey();
            obj.AsDynamic().USER2GROUPUSERGROUPCODE = ug.GetKey();
        }

        protected override void MakeSimpleChange(User2Group obj)
        {
            //NOTE: менять нечего
        }

        protected override void CheckSimpleChange(User2Group source, User2Group dest)
        {
            //NOTE: менять нечего
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();

            _userTest.ClearForSelf();
            _userGroupTest.ClearForSelf();
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<User>(TestDecimal, TestString);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
