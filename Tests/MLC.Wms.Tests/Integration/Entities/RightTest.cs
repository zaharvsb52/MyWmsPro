using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class RightTest : BaseEntityTest<Right>
    {
        public const string ExistsItem1Code = "TST_RIGHT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "RightDesc";
        }

        protected override void FillRequiredFields(Right entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.RightLocked = TestBool;
        }

        [Test, Ignore("На PRD1 нет Transact")]
        public override void Entity_should_be_create_read_update_delete()
        {
            //base.Entity_should_be_create_read_update_delete();
        }
    }
}