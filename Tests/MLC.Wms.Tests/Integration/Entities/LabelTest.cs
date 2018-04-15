using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class LabelTest : BaseEntityTest<Label>
    {
        public const string ExistsItem1Code = "TST_LABEL_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Label.LabelNamePropertyName;
        }

        protected override void FillRequiredFields(Label entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.REPORT_R = ReportTest.ExistsItem1Code;
            obj.LABELNAME = TestString;
        }

        [Test, Ignore("http://mp-ts-nwms/issue/wmsMLC-11554")]
        public override void Entity_should_be_create_read_update_delete()
        {
            base.Entity_should_be_create_read_update_delete();
        }
    }
}