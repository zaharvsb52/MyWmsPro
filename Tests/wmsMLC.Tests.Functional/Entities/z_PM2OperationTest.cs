using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PM2OperationTest : BaseWMSObjectTest<PM2Operation>
    {
        private readonly PMTest _pmTest = new PMTest();
        private readonly BillOperationTest _operationTest = new BillOperationTest();

        public PM2OperationTest()
        {
            _pmTest.TestString = TestString;
            _operationTest.TestString = TestString;
        }

        protected override void FillRequiredFields(PM2Operation obj)
        {
            base.FillRequiredFields(obj);

            var pm = _pmTest.CreateNew();
            var oper = _operationTest.CreateNew();

            obj.AsDynamic().PM2OPERATIONCODE = TestString;
            obj.AsDynamic().PM2OPERATIONPMCODE = pm.GetKey();
            obj.AsDynamic().PM2OPERATIONOPERATIONCODE = oper.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PM2OPERATIONCODE = '{0}')", TestString);
        }
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _operationTest, _pmTest };
        }
    }
}