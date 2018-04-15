using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class State2OperationTest : BaseWMSObjectTest<State2Operation>
    {
        private readonly StatusTest _statusTest = new StatusTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _statusTest, _billOperationTest };
        }

        protected override void FillRequiredFields(State2Operation obj)
        {
            base.FillRequiredFields(obj);

            var status = _statusTest.CreateNew();
            var operation = _billOperationTest.CreateNew();
            var mgr = IoC.Instance.Resolve<ISysObjectManager>();

            obj.AsDynamic().STATE2OPERATIONID = TestDecimal;
            obj.AsDynamic().STATE2OPERATIONOBJECTNAME = mgr.Get(0).ObjectName;
            obj.AsDynamic().STATE2OPERATIONOPERATIONCODE = operation.GetKey();
            obj.AsDynamic().STATE2OPERATIONSTATUSCODE = status.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STATE2OPERATIONID = '{0}')", TestDecimal);
        }
    }
}