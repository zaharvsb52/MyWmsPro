using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillCalcConfigTest : BaseWMSObjectTest<BillCalcConfig>
    {
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();
        private readonly PattCalcDataSourceTest _pattCalcDataSourceTest = new PattCalcDataSourceTest();
        private readonly BillBillEntityTest _billBillEntityTest = new BillBillEntityTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest, _billOperation2ContractTest, _pattCalcDataSourceTest, _billBillEntityTest };
        }

        protected override void FillRequiredFields(BillCalcConfig obj)
        {
            base.FillRequiredFields(obj);

            _billBillerTest.TestDecimal = TestDecimal + 1;
            _billBillerTest.TestString = TestString + "1";

            obj.AsDynamic().CALCCONFIGID = TestDecimal;
            obj.AsDynamic().BILLERCODE_R = _billBillerTest.CreateNew().GetKey();
            obj.AsDynamic().OPERATION2CONTRACTID_R = _billOperation2ContractTest.CreateNew().GetKey();
            obj.AsDynamic().CALCDATASOURCECODE_R = _pattCalcDataSourceTest.CreateNew().GetKey();
            obj.AsDynamic().BILLENTITYCODE_R = _billBillEntityTest.CreateNew().GetKey();
            obj.AsDynamic().CALCCONFIGNAME = TestString;
            obj.AsDynamic().CALCCONFIGLOCKED = false;
            obj.AsDynamic().CALCCONFIGPROCBEFORE = TestString;
            obj.AsDynamic().CALCCONFIGPROCAFTER = TestString;
            obj.AsDynamic().CALCCONFIGPROCVERIFICATION = TestString;
            obj.AsDynamic().CALCCONFIGPROCCALC = TestString;
            obj.AsDynamic().CALCCONFIGPROCSTRATEGY = TestString;
            obj.AsDynamic().CALCCONFIGDATEFROM = DateTime.Now;
            obj.AsDynamic().CALCCONFIGDATETILL = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCCONFIGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillCalcConfig obj)
        {
            obj.AsDynamic().CALCCONFIGDESC = TestString;
        }

        protected override void CheckSimpleChange(BillCalcConfig source, BillCalcConfig dest)
        {
            string sourceName = source.AsDynamic().CALCCONFIGDESC;
            string destName = dest.AsDynamic().CALCCONFIGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}