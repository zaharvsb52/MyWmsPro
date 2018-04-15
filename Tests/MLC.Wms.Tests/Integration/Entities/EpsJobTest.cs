using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EpsJobTest : BaseEntityTest<EpsJob>
    {
        public const string ExistsItem1Code = "TST_EPSJOB_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "JOBNAME";
        }

        protected override void FillRequiredFields(EpsJob entity)
        {
            base.FillRequiredFields(entity);
            entity.JobLocked = TestBool;
        }
    }
}