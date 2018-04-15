using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WorkingTest : BaseEntityTest<Working>
    {
        public const decimal ExistsItem1Id = -1;
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WorkingDoc";
        }

        protected override void FillRequiredFields(Working entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WorkID_r = WorkTest.ExistsItem1Id;
            obj.WorkerID_r = WorkerTest.ExistsItem1Id;
            obj.WorkingFrom = TestDateTime;
            obj.WorkingMult = TestDecimal;
        }
    }
}