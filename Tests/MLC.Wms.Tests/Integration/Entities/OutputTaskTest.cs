using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture, Ignore("Менеджер др.")]
    public class OutputTaskTest : BaseEntityTest<OutputTask>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OutputTaskStatus";

            HaveHistory = false;
        }

        protected override void FillRequiredFields(OutputTask entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OutputID_r = OutputTest.ExistsItem1Id;
            obj.OutputTaskCode = TestString;
            obj.OutputTaskOrder = TestDecimal;
        }
    }
}