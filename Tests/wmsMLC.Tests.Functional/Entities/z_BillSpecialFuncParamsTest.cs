using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillSpecialFuncParamsTest : BaseWMSObjectTest<BillSpecialFuncParams>
    {
        private readonly BillSpecialFunctionTest _billSpecialFunctionTest = new BillSpecialFunctionTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billSpecialFunctionTest };
        }

        protected override void FillRequiredFields(BillSpecialFuncParams obj)
        {
            base.FillRequiredFields(obj);

            var func = _billSpecialFunctionTest.CreateNew();

            obj.AsDynamic().SPECIALFUNCTIONPARAMSID = TestDecimal;
            obj.AsDynamic().SPECIALFUNCTIONCODE_R = func.GetKey();
            obj.AsDynamic().SPECIALFUNCTIONPARAMSNAME = TestString;
            obj.AsDynamic().SPECIALFUNCTIONPARAMSORDER = TestDecimal;
            obj.AsDynamic().SPECIALFUNCTIONPARAMSLOOKUP = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SPECIALFUNCTIONPARAMSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillSpecialFuncParams obj)
        {
            obj.AsDynamic().SPECIALFUNCTIONPARAMSDESC = TestString;
        }

        protected override void CheckSimpleChange(BillSpecialFuncParams source, BillSpecialFuncParams dest)
        {
            string sourceName = source.AsDynamic().SPECIALFUNCTIONPARAMSDESC;
            string destName = dest.AsDynamic().SPECIALFUNCTIONPARAMSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}