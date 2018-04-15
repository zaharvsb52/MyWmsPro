using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class UserTest : BaseEntityTest<User>
    {
        public const string ExistsItem1Code = "TST_USER_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = User.UserMiddleNamePropertyName;
        }

        protected override void FillRequiredFields(User entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.LOGIN = TestString;
            obj.USERLASTNAME = TestString;
            obj.USERNAME = TestString;
            obj.LANGCODE_R = "RUS";
        }

        [Test, Ignore("На PRD1 нет Transact")]
        public override void Entity_should_be_create_read_update_delete()
        {
            //base.Entity_should_be_create_read_update_delete();
        }
    }
}