using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class UserGroupTest : BaseEntityTest<UserGroup>
    {
        public const string ExistsItem1Code = "TST_USERGROUP_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "UserGroupDesc";

            HaveHistory = false;
        }

        protected override void FillRequiredFields(UserGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.UserGroupLocked = false;
            obj.UserGroupName = TestString;
            obj.UserGroupCode = TestString;
        }

        [Test, Ignore("На PRD1 нет Transact")]
        public override void Entity_should_be_create_read_update_delete()
        {
            //base.Entity_should_be_create_read_update_delete();
        }

    }
}