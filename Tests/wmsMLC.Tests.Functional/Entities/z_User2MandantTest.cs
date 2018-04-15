using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class User2MandantTest : BaseWMSObjectTest<User2Mandant>
    {
        private readonly UserTest _userTest = new UserTest();
       // private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] {_userTest/*, _mandantTest*/};
        }

        protected override void FillRequiredFields(User2Mandant obj)
        {
            base.FillRequiredFields(obj);


            var user = _userTest.CreateNew();
            //var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().USER2MANDANTID = TestDecimal;
            obj.AsDynamic().USER2MANDANTUSERCODE = user.GetKey();
            obj.AsDynamic().MANDANTID = 1;// mandant.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USER2MANDANTID = '{0}')", TestDecimal);
        }
    }
}