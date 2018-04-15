using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillSpecialFuncEntityTest : BaseWMSObjectTest<BillSpecialFuncEntity>
    {
        private readonly BillSpecialFunctionTest _billSpecialFunctionTest = new BillSpecialFunctionTest();


        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billSpecialFunctionTest };
        }

        protected override void FillRequiredFields(BillSpecialFuncEntity obj)
        {
            base.FillRequiredFields(obj);

            var func = _billSpecialFunctionTest.CreateNew();

            obj.AsDynamic().SPECIALFUNCTIONENTITYID = TestDecimal;
            obj.AsDynamic().SPECIALFUNCTIONCODE_R = func.GetKey();
            obj.AsDynamic().SPECIALFUNCENTITYOBJECTENTITY = "WAREHOUSE";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SPECIALFUNCTIONENTITYID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillSpecialFuncEntity obj)
        {
            obj.AsDynamic().SPECIALFUNCTIONENTITYBODY = TestString;
        }

        protected override void CheckSimpleChange(BillSpecialFuncEntity source, BillSpecialFuncEntity dest)
        {
            string sourceName = source.AsDynamic().SPECIALFUNCTIONENTITYBODY;
            string destName = dest.AsDynamic().SPECIALFUNCTIONENTITYBODY;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}