using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BPBatchTest : BaseEntityTest<BPBatch>
    {
        public const string ExistsItem1Code = "TST_BPBATCH_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BPBatch.BatchNamePropertyName;
        }

        protected override void FillRequiredFields(BPBatch entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BATCHNAME = TestString;
            obj.WORKFLOWCODE_R = BPWorkflowTest.ExistsItem1Code;
        }
    }
}