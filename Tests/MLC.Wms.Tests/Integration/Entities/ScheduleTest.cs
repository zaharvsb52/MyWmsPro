using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ScheduleTest : BaseEntityTest<Schedule>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ScheduleDesc";
        }
        protected override void FillRequiredFields(Schedule entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.JobCode_r = EpsJobTest.ExistsItem1Code;
            obj.ScheduleCron = TestString;
            obj.ScheduleName = TestString;
        }
    }
}