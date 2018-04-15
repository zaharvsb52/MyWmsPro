using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PMConfigTest : BaseWMSObjectTest<PMConfig>
    {
        private readonly PM2OperationTest _pm2OperationTest = new PM2OperationTest();
        private readonly PMMethodTest _pmMethodTest = new PMMethodTest();

        protected override void FillRequiredFields(PMConfig obj)
        {
            base.FillRequiredFields(obj);

            var pm2Operation = _pm2OperationTest.CreateNew();
            var pmMethod = _pmMethodTest.CreateNew();

            obj.AsDynamic().PMCONFIGID = TestDecimal;
            obj.AsDynamic().PM2OPERATIONCODE_R = pm2Operation.GetKey();
            obj.AsDynamic().OBJECTENTITYCODE_R = "PRODUCT";
            obj.AsDynamic().OBJECTNAME_R = "SYSENUM";
            obj.AsDynamic().PMMETHODCODE_R = pmMethod.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PMCONFIGID = '{0}')", TestDecimal);
        }
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pm2OperationTest, _pmMethodTest };
        }
    }
}