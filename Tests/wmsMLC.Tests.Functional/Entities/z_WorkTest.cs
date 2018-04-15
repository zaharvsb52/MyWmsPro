using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WorkTest : BaseWMSObjectTest<Work>
    {
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();
        private readonly StatusTest _statusTest = new StatusTest();
        private readonly ClientSessionTest _clientSessionTest = new ClientSessionTest();
        private readonly WorkGroupTest _workGroupTest = new WorkGroupTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billOperationTest, _statusTest, _clientSessionTest, _workGroupTest };
        }

        protected override void FillRequiredFields(Work obj)
        {
            base.FillRequiredFields(obj);

            var operation = _billOperationTest.CreateNew();
            var status = _statusTest.CreateNew();
            var sesssion = _clientSessionTest.CreateNew();
            var workGroup = _workGroupTest.CreateNew();

            obj.SetProperty(obj.GetPrimaryKeyPropertyName(), TestDecimal);
            obj.AsDynamic().OPERATIONCODE_R = operation.GetKey();
            obj.AsDynamic().STATUSCODE_R = status.GetKey();
            obj.AsDynamic().CLIENTSESSIONID_R = sesssion.GetKey();
            obj.AsDynamic().WORKGROUPID_R = workGroup.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Work obj)
        {
            obj.AsDynamic().WORKDESC = TestString;
        }

        protected override void CheckSimpleChange(Work source, Work dest)
        {
            string sourceName = source.AsDynamic().WORKDESC;
            string destName = dest.AsDynamic().WORKDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}