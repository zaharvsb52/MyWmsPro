using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    class EpsTaskTest : BaseEntityTest<EpsTask>
    {
        public const string ExistsItem1Code = "TST_EPSTASK_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TASKDESC";
        }

        protected override void FillRequiredFields(EpsTask entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.AsDynamic().TASKCODE = TestString;
            obj.AsDynamic().TASKNAME = TestString;
            obj.AsDynamic().TASKLOCKED = TestBool;
            obj.AsDynamic().TASKTYPE = SysEnumTest.ExistsItem1Id;
        }
    }
}