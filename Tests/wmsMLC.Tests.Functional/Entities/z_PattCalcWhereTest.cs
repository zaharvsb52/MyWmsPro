using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PattCalcWhereTest : BaseWMSObjectTest<PattCalcWhere>
    {
        private readonly PattCalcDataSourceTest _pattCalcDataSourceTest = new PattCalcDataSourceTest();
        private readonly PattTWhereSectionTest _pattTWhereSectionTest = new PattTWhereSectionTest();
        private readonly BillSpecialFunctionTest _billSpecialFunctionTest = new BillSpecialFunctionTest();  

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _pattCalcDataSourceTest, _pattTWhereSectionTest, _billSpecialFunctionTest };
        }

        protected override void FillRequiredFields(PattCalcWhere obj)
        {
            base.FillRequiredFields(obj);

            _pattCalcDataSourceTest.TestDecimal = TestDecimal + 1;
            _pattCalcDataSourceTest.TestString = TestString + "1";

            obj.AsDynamic().CALCWHEREID = TestDecimal;
            obj.AsDynamic().CALCDATASOURCECODE_R = _pattCalcDataSourceTest.CreateNew().GetKey();
            obj.AsDynamic().TEMPLATEWHERESECTIONID_R = _pattTWhereSectionTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCWHEREID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PattCalcWhere obj)
        {
            obj.AsDynamic().CALCWHEREPARAM1 = TestString;
        }

        protected override void CheckSimpleChange(PattCalcWhere source, PattCalcWhere dest)
        {
            string sourceName = source.AsDynamic().CALCWHEREPARAM1;
            string destName = dest.AsDynamic().CALCWHEREPARAM1;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}