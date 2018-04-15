using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillCalcEventConfigTest : BaseWMSObjectTest<BillCalcEventConfig>
    {
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();
        private readonly PattCalcDataSourceTest _pattCalcDataSourceTest = new PattCalcDataSourceTest();
        private readonly BillBillEntityTest _billBillEntityTest = new BillBillEntityTest();
        private readonly EventKindTest _eventKindTest = new EventKindTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billBillerTest, _billOperation2ContractTest, _pattCalcDataSourceTest, _billBillEntityTest, _eventKindTest };
        }

        protected override void FillRequiredFields(BillCalcEventConfig obj)
        {
            base.FillRequiredFields(obj);

            _billBillerTest.TestDecimal = TestDecimal + 1;
            _billBillerTest.TestString = TestString + "1";

            obj.AsDynamic().CALCEVENTCONFIGID = TestDecimal;
            obj.AsDynamic().BILLERCODE_R = _billBillerTest.CreateNew().GetKey();
            obj.AsDynamic().OPERATION2CONTRACTID_R = _billOperation2ContractTest.CreateNew().GetKey();
            obj.AsDynamic().CALCDATASOURCECODE_R = _pattCalcDataSourceTest.CreateNew().GetKey();
            obj.AsDynamic().BILLENTITYCODE_R = _billBillEntityTest.CreateNew().GetKey();
            obj.AsDynamic().CALCEVENTCONFIGNAME = TestString;
            obj.AsDynamic().CALCEVENTCONFIGFROM = DateTime.Now;
            obj.AsDynamic().CALCEVENTCONFIGFROMTILL = DateTime.Now;
            obj.AsDynamic().CALCEVENTCONFIGFIELDDATE = TestString;
            obj.AsDynamic().CALCEVENTCONFIGLOCKED = false;
            obj.AsDynamic().EVENTKINDCODE_R = _eventKindTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALCEVENTCONFIGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillCalcEventConfig obj)
        {
            obj.AsDynamic().CALCEVENTCONFIGDESC = TestString;
        }

        protected override void CheckSimpleChange(BillCalcEventConfig source, BillCalcEventConfig dest)
        {
            string sourceName = source.AsDynamic().CALCEVENTCONFIGDESC;
            string destName = dest.AsDynamic().CALCEVENTCONFIGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}