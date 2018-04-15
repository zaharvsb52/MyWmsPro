using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class QlfDetailTest : BaseEntityTest<QlfDetail>
    {
        public const string ExistsItem1Code = "TST_QLFDETAIL_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "QLFDETAILDESC";
        }

        [Test, Ignore("Нет transact - http://mp-ts-nwms/issue/wmsMLC-11581")]
        public override void Entity_should_be_create_read_update_delete()
        {
            //base.Entity_should_be_create_read_update_delete();
        }

        protected override void FillRequiredFields(QlfDetail entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.QLFDETAILNAME = TestString;
        }
    }
}