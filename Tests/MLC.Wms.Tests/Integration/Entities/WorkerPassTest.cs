using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WorkerPassTest : BaseEntityTest<WorkerPass>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WorkerPassAgency";
        }

        protected override void FillRequiredFields(WorkerPass entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WorkerID_r = WorkerTest.ExistsItem1Id;
            obj.WorkerPassType = TestString;
            obj.CountryCode_r = IsoCountryTest.ExistsItem1Code;

        }
    }
}