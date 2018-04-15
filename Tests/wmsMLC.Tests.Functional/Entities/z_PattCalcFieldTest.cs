using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattCalcFieldTest : BaseWMSObjectTest<PattCalcField>
    {
        private readonly PattCalcDataSourceTest _pattCalcDataSourceTest = new PattCalcDataSourceTest();
        private readonly PattTFieldSectionTest _pattTFieldSectionTest = new PattTFieldSectionTest();
        private readonly BillSpecialFunctionTest _billSpecialFunctionTest = new BillSpecialFunctionTest();  

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattCalcDataSourceTest, _pattTFieldSectionTest, _billSpecialFunctionTest };
        }

        protected override void FillRequiredFields(PattCalcField obj)
        {
            base.FillRequiredFields(obj);

            _pattCalcDataSourceTest.TestString = TestString + "1";
            _pattCalcDataSourceTest.TestDecimal = TestDecimal + 1;

            obj.AsDynamic().CALCFIELDID = TestDecimal;
            obj.AsDynamic().CALCDATASOURCECODE_R = _pattCalcDataSourceTest.CreateNew().GetKey();
            obj.AsDynamic().TEMPLATEFIELDSECTIONID_R = _pattTFieldSectionTest.CreateNew().GetKey();
            obj.AsDynamic().SPECIALFUNCTIONCODE_R = _billSpecialFunctionTest.CreateNew().GetKey();
            obj.AsDynamic().CALCFIELDALIAS = TestString;
            obj.AsDynamic().CALCFIELDFUNCTIONPARAM2 = TestString;
            obj.AsDynamic().CALCFIELDFUNCTIONPARAM3 = TestString;
            obj.AsDynamic().CALCFIELDFUNCTIONPARAM4 = TestString;
            obj.AsDynamic().CALCFIELDFUNCTIONPARAM5 = TestString;
            obj.AsDynamic().CALCFIELDOBJECTENTITY = TestString;
            obj.AsDynamic().CALCFIELDOBJECTATTR = TestString;
            obj.AsDynamic().CALCFIELDALIASENTITY = TestString;
            obj.AsDynamic().CALCFIELDEXPRESSION = TestString;
            obj.AsDynamic().CALCFIELDDATATYPE = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCFIELDID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattCalcField obj)
        {
            obj.AsDynamic().CALCFIELDFUNCTIONPARAM1 = TestString;
        }

        protected override void CheckSimpleChange(PattCalcField source, PattCalcField dest)
        {
            string sourceName = source.AsDynamic().CALCFIELDFUNCTIONPARAM1;
            string destName = dest.AsDynamic().CALCFIELDFUNCTIONPARAM1;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}