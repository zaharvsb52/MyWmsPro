using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Worker2GroupTest : BaseWMSObjectTest<Worker2Group>
    {
        private readonly WorkerGroupTest _workerGroupTest = new WorkerGroupTest();
        private readonly WorkerTest _workerTest = new WorkerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] {_workerGroupTest, _workerTest};
        }

        protected override void FillRequiredFields(Worker2Group obj)
        {
            base.FillRequiredFields(obj);

            var group = _workerGroupTest.CreateNew();
            var worker = _workerTest.CreateNew();

            obj.AsDynamic().WORKER2GROUPID = TestDecimal;
            obj.AsDynamic().WORKER2GROUPWORKERGROUPID = group.GetKey();
            obj.AsDynamic().WORKER2GROUPWORKERID = worker.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKER2GROUPID = '{0}')", TestDecimal);
        }
    }
}