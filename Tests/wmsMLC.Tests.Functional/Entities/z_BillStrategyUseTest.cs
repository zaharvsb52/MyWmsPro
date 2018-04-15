using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillStrategyUseTest : BaseWMSObjectTest<BillStrategyUse>
    {
        private readonly BillStrategyTest _billStrategyTest = new BillStrategyTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billStrategyTest, _billOperation2ContractTest };
        }

        protected override void FillRequiredFields(BillStrategyUse obj)
        {
            base.FillRequiredFields(obj);

            var strategy = _billStrategyTest.CreateNew();
            var operation = _billOperation2ContractTest.CreateNew();

            obj.AsDynamic().STRATEGYUSEID = TestDecimal;
            obj.AsDynamic().STRATEGYCODE_R = strategy.GetKey();
            obj.AsDynamic().OPERATION2CONTRACTID_R = operation.GetKey();
            obj.AsDynamic().STRATEGYUSENAME = TestString;
            obj.AsDynamic().STRATEGYUSEORDER = TestDecimal;
            obj.AsDynamic().STRATEGYUSEFROM = DateTime.Now;
            obj.AsDynamic().STRATEGYUSETILL = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STRATEGYUSEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillStrategyUse obj)
        {
            obj.AsDynamic().STRATEGYUSEDESC = TestString;
        }

        protected override void CheckSimpleChange(BillStrategyUse source, BillStrategyUse dest)
        {
            string sourceName = source.AsDynamic().STRATEGYUSEDESC;
            string destName = dest.AsDynamic().STRATEGYUSEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}