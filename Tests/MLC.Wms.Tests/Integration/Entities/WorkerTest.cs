using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WorkerTest : BaseEntityTest<Worker>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Worker.WorkerLastNamePropertyName;
        }

        protected override void FillRequiredFields(Worker entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORKERLASTNAME = TestString;
        }
    }
}