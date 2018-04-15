using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillStrategyParamsTest : BaseWMSObjectTest<BillStrategyParams>
    {
        private readonly BillStrategyTest _billStrategyTest = new BillStrategyTest();


        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billStrategyTest };
        }

        protected override void FillRequiredFields(BillStrategyParams obj)
        {
            base.FillRequiredFields(obj);

            var strategy = _billStrategyTest.CreateNew();

            obj.AsDynamic().STRATEGYPARAMSID = TestDecimal;
            obj.AsDynamic().STRATEGYCODE_R = strategy.GetKey();
            obj.AsDynamic().STRATEGYPARAMSNAME = TestString;
            obj.AsDynamic().STRATEGYPARAMSDATATYPE = TestDecimal;
            obj.AsDynamic().STRATEGYPARAMSMUSTSET = false;
            obj.AsDynamic().STRATEGYPARAMSINDEX = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STRATEGYPARAMSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillStrategyParams obj)
        {
            obj.AsDynamic().STRATEGYPARAMSDESC = TestString;
        }

        protected override void CheckSimpleChange(BillStrategyParams source, BillStrategyParams dest)
        {
            string sourceName = source.AsDynamic().STRATEGYPARAMSDESC;
            string destName = dest.AsDynamic().STRATEGYPARAMSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}