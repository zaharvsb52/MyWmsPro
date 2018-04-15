using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillOperation2ContractTest : BaseWMSObjectTest<BillOperation2Contract>
    {
        private readonly BillContractTest _billContractTest = new BillContractTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly BillAnalyticsTest _billAnalyticsTest = new BillAnalyticsTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billContractTest, _billOperationTest, _billAnalyticsTest, _billBillerTest,  };
        }

        protected override void FillRequiredFields(BillOperation2Contract obj)
        {
            base.FillRequiredFields(obj);

            var contract = _billContractTest.CreateNew();
            var operation = _billOperationTest.CreateNew();
            var biller = _billBillerTest.CreateNew();
            var analytics = _billAnalyticsTest.CreateNew();

            obj.AsDynamic().OPERATION2CONTRACTID = TestDecimal;
            obj.AsDynamic().BILLOPERATION2CONTRACTCONTRACTID = contract.GetKey();
            obj.AsDynamic().BILLOPERATION2CONTRACTOPERATIONCODE = operation.GetKey();
            obj.AsDynamic().OPERATION2CONTRACTNAME = TestString;
            obj.AsDynamic().BILLOPERATION2CONTRACTBILLERCODE = biller.GetKey();
            obj.AsDynamic().BILLOPERATION2CONTRACTANALYTICSCODE = analytics.GetKey();
            obj.AsDynamic().OPERATION2CONTRACTCODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OPERATION2CONTRACTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillOperation2Contract obj)
        {
            obj.AsDynamic().OPERATION2CONTRACTDESC = TestString;
        }

        protected override void CheckSimpleChange(BillOperation2Contract source, BillOperation2Contract dest)
        {
            string sourceName = source.AsDynamic().OPERATION2CONTRACTDESC;
            string destName = dest.AsDynamic().OPERATION2CONTRACTDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}