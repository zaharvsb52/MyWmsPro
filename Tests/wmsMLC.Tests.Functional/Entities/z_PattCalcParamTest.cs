using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattCalcParamTest : BaseWMSObjectTest<PattCalcParam>
    {
        private readonly PattCalcDataSourceTest _calcDataSourceTest = new PattCalcDataSourceTest();
        private readonly PattTParamsTest _pattTParamsTest = new PattTParamsTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _calcDataSourceTest, _pattTParamsTest };
        }

        protected override void FillRequiredFields(PattCalcParam obj)
        {
            base.FillRequiredFields(obj);

            _pattTParamsTest.TestDecimal = TestDecimal + 1;
            _pattTParamsTest.TestString = TestString + "1";

            obj.AsDynamic().CALCPARAMID = TestDecimal;
            obj.AsDynamic().CalcDataSourceCode_r = _calcDataSourceTest.CreateNew().GetKey();
            obj.AsDynamic().TemplateParamsID_r = _pattTParamsTest.CreateNew().GetKey();
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCPARAMID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattCalcParam obj)
        {
            obj.AsDynamic().CALCPARAMVALUE = TestString;
        }

        protected override void CheckSimpleChange(PattCalcParam source, PattCalcParam dest)
        {
            string sourceName = source.AsDynamic().CALCPARAMVALUE;
            string destName = dest.AsDynamic().CALCPARAMVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}