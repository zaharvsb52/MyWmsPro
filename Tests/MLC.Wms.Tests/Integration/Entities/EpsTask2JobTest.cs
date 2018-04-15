using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    class EpsTask2JobTest : BaseEntityTest<EpsTask2Job>
    {
        //public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = EpsTask2Job.Task2JobOrderPropertyName;
        }

        protected override void FillRequiredFields(EpsTask2Job entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.AsDynamic().EPSTASK2JOBJOBCODE = EpsJobTest.ExistsItem1Code;
            obj.AsDynamic().EPSTASK2JOBTASKCODE = EpsTaskTest.ExistsItem1Code;
            obj.AsDynamic().TASK2JOBORDER = TestDecimal;
        }
    }
}