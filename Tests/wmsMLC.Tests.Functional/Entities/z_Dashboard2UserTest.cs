using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Dashboard2UserTest : BaseWMSObjectTest<Dashboard2User>
    {
        private readonly DashboardTest _dashboardTest = new DashboardTest();
        private readonly UserTest _userTest = new UserTest();
        private readonly UserGroupTest _userGroup = new UserGroupTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _dashboardTest, _userTest, _userGroup };
        }

        protected override void FillRequiredFields(Dashboard2User obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().DASHBOARD2USERID = TestDecimal;
            obj.AsDynamic().DASHBOARD2USERDASHBOARDCODE = _dashboardTest.CreateNew().GetKey();
            obj.AsDynamic().DASHBOARD2USERUSERCODE = _userTest.CreateNew().GetKey();
            obj.AsDynamic().DASHBOARD2USERUSERGROUPCODE = _userGroup.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(DASHBOARD2USERID = '{0}')", TestDecimal);
        }
    }
}