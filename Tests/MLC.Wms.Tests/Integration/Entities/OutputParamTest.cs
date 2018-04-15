using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture, Ignore("Нет своего менеджера")]
    public class OutputParamTest : BaseEntityTest<OutputParam>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OutputParamValue";
            HaveHistory = false;
        }

        protected override void FillRequiredFields(OutputParam entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OutputID_r = OutputTest.ExistsItem1Id;
            obj.OutputParamCode = TestString;
            obj.OutputParamType = EpsParamType.EPS;
        }


        [Test, Ignore("Нет своего менеджера")]
        public override void Filter_should_return_empty_collections()
        {
            base.Entity_should_be_create_read_update_delete();
        }


        [Test, Ignore("Нет своего менеджера")]
        public override void Filter_should_return_non_empty_collections()
        {
            base.Entity_should_be_create_read_update_delete();
        }

        [Test, Ignore("Нет своего менеджера")]
        public override void Filter_should_work_by_all_entity_fields()
        {
            base.Entity_should_be_create_read_update_delete();
        }
    }
}