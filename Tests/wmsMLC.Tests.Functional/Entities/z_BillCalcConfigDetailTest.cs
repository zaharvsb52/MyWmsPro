using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillCalcConfigDetailTest : BaseWMSObjectTest<BillCalcConfigDetail>
    {
        private readonly BillCalcConfigTest _billCalcConfigTest = new BillCalcConfigTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billCalcConfigTest };
        }

        protected override void FillRequiredFields(BillCalcConfigDetail obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CALCCONFIGDETAILID = TestDecimal;
            obj.AsDynamic().CALCCONFIGID_R = _billCalcConfigTest.CreateNew().GetKey();
            obj.AsDynamic().CALCCONFIGDETAILDESTINATION = TestString;
            obj.AsDynamic().CALCCONFIGDETAILFIELDROUND = TestString;
            obj.AsDynamic().CALCCONFIGDETAILFIELDROUNDRATE = TestString;
            obj.AsDynamic().CALCCONFIGDETAILFUNC = TestString;
            obj.AsDynamic().CALCCONFIGDETAILFUNCROUND = TestString;
            obj.AsDynamic().CALCCONFIGDETAILFUNCROUNDRATE = TestString;
            obj.AsDynamic().CALCCONFIGDETAILEXPRESSION = TestString;
            obj.AsDynamic().CALCCONFIGDETAILCORRECTRATE = TestDouble;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCCONFIGDETAILID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillCalcConfigDetail obj)
        {
            obj.AsDynamic().CALCCONFIGDETAILFIELDSOURCE = TestString;
        }

        protected override void CheckSimpleChange(BillCalcConfigDetail source, BillCalcConfigDetail dest)
        {
            string sourceName = source.AsDynamic().CALCCONFIGDETAILFIELDSOURCE;
            string destName = dest.AsDynamic().CALCCONFIGDETAILFIELDSOURCE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}