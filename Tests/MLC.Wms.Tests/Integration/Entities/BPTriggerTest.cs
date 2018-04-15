using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BPTriggerTest : BaseEntityTest<BPTrigger>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BPTrigger.TriggerExcpressionPropertyName;
        }

        protected override void FillRequiredFields(BPTrigger entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PROCESSCODE_R = BPProcessTest.ExistsItem1Code;
            obj.OBJECTNAME_R = "BPTRIGGER";
        }
    }
}