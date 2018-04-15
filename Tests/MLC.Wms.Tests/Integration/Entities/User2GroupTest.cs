using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture, Ignore("Нет Transact")]
    public class User2GroupTest : BaseEntityTest<User2Group>
    {
        protected override void FillRequiredFields(User2Group entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.USER2GROUPUSERCODE = UserTest.ExistsItem1Code;
            obj.USER2GROUPUSERGROUPCODE = UserGroupTest.ExistsItem1Code;
        }
    }
}