using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WorkerGroupTest : BaseEntityTest<WorkerGroup>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WorkerGroupDesc";
        }

        protected override void FillRequiredFields(WorkerGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WorkerGroupName = TestString;
        }
    }
}