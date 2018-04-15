using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillEvent2OperationTest : BaseWMSObjectTest<BillEvent2Operation>
    {
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();
        private readonly EventKindTest _eventKindTest = new EventKindTest();
        private const string Business = "UNKNOWN";

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billOperationTest, _eventKindTest }; 
        }

        protected override void FillRequiredFields(BillEvent2Operation obj)
        {
            base.FillRequiredFields(obj);

            var operation = _billOperationTest.CreateNew();
            var eventKind = _eventKindTest.CreateNew();

            obj.AsDynamic().EVENT2OPERATIONID = TestDecimal;
            obj.AsDynamic().BILLEVENT2OPERATIONEVENTKINDCODE = eventKind.GetKey();
            obj.AsDynamic().BILLEVENT2OPERATIONOPERATIONCODE = operation.GetKey();
            obj.AsDynamic().PRIORITY = TestDecimal;
            obj.AsDynamic().EVENT2OPERATIONBUSINESS = Business;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENT2OPERATIONID = '{0}')", TestDecimal);
        }
    }
}