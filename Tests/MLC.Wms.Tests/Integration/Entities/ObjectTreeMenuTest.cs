using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ObjectTreeMenuTest : BaseEntityTest<ObjectTreeMenu>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OBJECTTREEACTION";
        }

        protected override void FillRequiredFields(ObjectTreeMenu entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ObjectName_r = "SYSTEMTREEMENU";//SysObjectTest.ExistsItemEntityCode;
            obj.ObjectTreeName = TestString;
            obj.ObjectTreeMenuType = ObjectTreeMenuType.DCL;
        }


        [Test, Ignore("Ошибка получения истории")]
        public override void Entity_should_have_history()
        {
            base.Entity_should_be_create_read_update_delete();
        }
    }
}