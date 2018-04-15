using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillCalcVerificationTest : BaseWMSObjectTest<BillCalcVerification>
    {
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();
        private readonly PattCalcDataSourceTest _pattCalcDataSourceTest = new PattCalcDataSourceTest();
        private readonly BillBillEntityTest _billBillEntityTest = new BillBillEntityTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest, _billOperation2ContractTest, _pattCalcDataSourceTest, _billBillEntityTest };
        }

        protected override void FillRequiredFields(BillCalcVerification obj)
        {
            base.FillRequiredFields(obj);

            _billBillerTest.TestDecimal = TestDecimal + 1;
            _billBillerTest.TestString = TestString + "1";

            obj.AsDynamic().CALCVERIFICATIONID = TestDecimal;
            obj.AsDynamic().BILLERCODE_R = _billBillerTest.CreateNew().GetKey();
            obj.AsDynamic().OPERATION2CONTRACTID_R = _billOperation2ContractTest.CreateNew().GetKey();
            obj.AsDynamic().CALCDATASOURCECODE_R = _pattCalcDataSourceTest.CreateNew().GetKey();
            obj.AsDynamic().BILLENTITYCODE_R = _billBillEntityTest.CreateNew().GetKey();
            obj.AsDynamic().CALCVERIFICATIONNAME = TestString;
            obj.AsDynamic().CALCVERIFICATIONFROM = DateTime.Now;
            obj.AsDynamic().CALCVERIFICATIONTILL = DateTime.Now;
            obj.AsDynamic().CALCVERIFICATIONMESSAGE = TestString;
            obj.AsDynamic().CALCVERIFICATIONFIELDEXCEPTION = TestString;
            obj.AsDynamic().CALCVERIFICATIONLOCKED = false;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCVERIFICATIONID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillCalcVerification obj)
        {
            obj.AsDynamic().CALCVERIFICATIONDESC = TestString;
        }

        protected override void CheckSimpleChange(BillCalcVerification source, BillCalcVerification dest)
        {
            string sourceName = source.AsDynamic().CALCVERIFICATIONDESC;
            string destName = dest.AsDynamic().CALCVERIFICATIONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}