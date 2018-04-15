using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BPProcessTest : BaseEntityTest<BPProcess>
    {
        public const string ExistsItem1Code = "TST_BPPROCESS_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = BPProcess.NamePropertyName;
        }

        protected override void FillRequiredFields(BPProcess entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PROCESSNAME = TestString;
            obj.PROCESSEXECUTOR = TestString;
            obj.PROCESSENGINE = TestString;
            obj.PROCESSTYPE = TestString;
        }
    }
}