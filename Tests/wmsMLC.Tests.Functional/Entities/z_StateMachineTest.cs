using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class StateMachineTest : BaseWMSObjectTest<StateMachine>
    {
        private readonly StatusTest _statusTest = new StatusTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _statusTest, _billOperationTest };
        }

        protected override void FillRequiredFields(StateMachine obj)
        {
            base.FillRequiredFields(obj);

            var status = _statusTest.CreateNew();
            var operation = _billOperationTest.CreateNew();
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            obj.AsDynamic().STATEMACHINEID = TestDecimal;
            obj.AsDynamic().OBJECTNAME_R = mgr.Get(0).ObjectName;
            obj.AsDynamic().OPERATIONCODE_R = operation.GetKey();
            obj.AsDynamic().CURRENTSTATUS = status.GetKey();
            obj.AsDynamic().NEXTSTATUS = status.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STATEMACHINEID = '{0}')", TestDecimal);
        }
    }
}