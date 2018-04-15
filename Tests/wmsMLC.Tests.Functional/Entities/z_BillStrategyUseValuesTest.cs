using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillStrategyUseValuesTest : BaseWMSObjectTest<BillStrategyUseValues>
    {
        private readonly BillStrategyUseTest _billStrategyUseTest = new BillStrategyUseTest();
        private readonly BillStrategyParamsTest _billStrategyParamsTest =  new BillStrategyParamsTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billStrategyUseTest, _billStrategyParamsTest };
        }

        protected override void FillRequiredFields(BillStrategyUseValues obj)
        {
            base.FillRequiredFields(obj);

            var strategyUse = _billStrategyUseTest.CreateNew();

            _billStrategyParamsTest.TestDecimal = TestDecimal + 1;
            _billStrategyParamsTest.TestString = TestString + "1";
            var strategyParams = _billStrategyParamsTest.CreateNew();

            obj.AsDynamic().STRATEGYUSEVALUESID = TestDecimal;
            obj.AsDynamic().STRATEGYUSEID_R = strategyUse.GetKey();
            obj.AsDynamic().STRATEGYPARAMSID_R = strategyParams.GetKey();
            obj.AsDynamic().STRATEGYUSEVALUESVALUE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STRATEGYUSEVALUESID = '{0}')", TestDecimal);
        }
    }
}