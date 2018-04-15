using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BPWorkflowTest : BaseEntityTest<BPWorkflow>
    {
        public const string ExistsItem1Code = "TST_BPWORKFLOW_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BPWorkflow.WorkflowVersionPropertyName;
        }

        protected override void FillRequiredFields(BPWorkflow entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORKFLOWNAME = TestString;
        }
    }
}