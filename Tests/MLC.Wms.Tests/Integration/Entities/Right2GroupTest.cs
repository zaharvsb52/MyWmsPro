using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Right2GroupTest : BaseEntityTest<Right2Group>
    {
        protected override void FillRequiredFields(Right2Group entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.RIGHT2GROUPRIGHTCODE = RightTest.ExistsItem1Code;
            obj.RIGHT2GROUPRIGHTGROUPCODE = RightGroupTest.ExistsItem1Code;
        }

        [Test, Ignore("На PRD1 нет Transact")]
        public override void Entity_should_be_create_read_update_delete()
        {
            //base.Entity_should_be_create_read_update_delete();
        }
    }
}