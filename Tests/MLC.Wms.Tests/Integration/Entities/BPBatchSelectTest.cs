using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BPBatchSelectTest : BaseEntityTest<BPBatchSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PRIORITY";
        }

        protected override void FillRequiredFields(BPBatchSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BATCHCODE_R = BPBatchTest.ExistsItem1Code;
            obj.PRIORITY = TestDouble;
        }
    }
}