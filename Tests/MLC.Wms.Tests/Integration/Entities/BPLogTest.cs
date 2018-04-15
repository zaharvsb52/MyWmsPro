using System;
using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BPLogTest : BaseEntityTest<BPLog>
    {
        public const string ExistsItem1Code = "TST_BPLOG_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "BPLOGCURRENTSTEP";
        }

        protected override void FillRequiredFields(BPLog entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PROCESSCODE_R = BPProcessTest.ExistsItem1Code;
            obj.USERCODE_R = UserTest.ExistsItem1Code;
            obj.BPLOGINSTANCE = TestString;
            obj.BPLOGCURRENTSTEP = TestString;
            obj.BPLOGSTATUS = TestString;
            obj.BPLOGSTARTTIME = DateTime.Now;
            obj.BPLOGENDTIME = DateTime.Now;
        }

        [Test, Ignore("http://mp-ts-nwms/issue/wmsMLC-11609")]
        public override void Entity_should_have_history() { }
    }
}