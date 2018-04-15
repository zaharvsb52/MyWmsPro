using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Report2UserTest : BaseWMSObjectTest<Report2User>
    {
        private readonly ReportTest _reportTest = new ReportTest();
        private readonly UserTest _userTest = new UserTest();
        private readonly UserGroupTest _userGroupTest = new UserGroupTest();

        //public Report2UserTest()
        //{
        //    _reportTest.TestString = TestString;
        //    _userTest.TestString = TestString;
        //    _userGroupTest.TestString = TestString;
        //}

        protected override void FillRequiredFields(Report2User obj)
        {
            base.FillRequiredFields(obj);

            var r = _reportTest.CreateNew();
            var u = _userTest.CreateNew();
            var ug = _userGroupTest.CreateNew();

            obj.AsDynamic().REPORT2USERID = TestDecimal;
            obj.AsDynamic().REPORT2USERREPORT = r.GetKey();
            obj.AsDynamic().REPORT2USERUSERCODE = u.GetKey();
            obj.AsDynamic().REPORT2USERUSERGROUPCODE = ug.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(REPORT2USERID = '{0}')", TestDecimal);
        }

        //public override void ClearForSelf()
        //{
        //    base.ClearForSelf();
        //    _reportTest.ClearForSelf();
        //    _userTest.ClearForSelf();
        //    _userGroupTest.ClearForSelf();
        //}

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _reportTest, _userTest, _userGroupTest };
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<Report>(TestDecimal, TestString);
            DeleteByParent<User>(TestDecimal, TestString);
        }

        //[Test, Ignore("Зацикливание теста")]
        [Ignore("Зацикливание теста")]
        public override void ManagerGetFilteredTest()
        {

        }
        //[Test, Ignore("Зацикливание теста")]
        [Ignore("Зацикливание теста")]
        public override void ManagerCRUDTest()
        {

        }
        //[Test, Ignore("Зацикливание теста")]
        [Ignore("Зацикливание теста")]
        public override void ManagerGetAllTest()
        {

        }
        //[Test, Ignore("Зацикливание теста")]
        [Ignore("Зацикливание теста")]
        public override void DeleteByParent<TParent>(object childKey, object parentKey)
        {

        }


    }
}