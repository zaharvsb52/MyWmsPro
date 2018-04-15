using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillWorkActDetailTest : BaseWMSObjectTest<BillWorkActDetail>
    {
        private readonly BillWorkActTest _billWorkActTest = new BillWorkActTest();
        private readonly BillOperation2ContractTest _billOperation2ContractTest = new BillOperation2ContractTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billWorkActTest, _billOperation2ContractTest };
        }

        protected override void FillRequiredFields(BillWorkActDetail obj)
        {
            base.FillRequiredFields(obj);
            
            var workAct = _billWorkActTest.CreateNew();
            _billOperation2ContractTest.TestDecimal = TestDecimal + 3;
            _billOperation2ContractTest.TestString = TestString + "3";
            var o2c = _billOperation2ContractTest.CreateNew();

            obj.AsDynamic().WORKACTDETAILID = TestDecimal;
            obj.AsDynamic().WORKACTID_R = workAct.GetKey();
            obj.AsDynamic().OPERATION2CONTRACTID_R = o2c.GetKey(); 
            obj.AsDynamic().WORKACTDETAILMANUAL = false;
            obj.AsDynamic().WORKACTDETAILCAUSE1 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE2 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE3 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE4 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE5 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE6 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE7 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE8 = TestString;
            obj.AsDynamic().WORKACTDETAILCAUSE9 = TestString;
            obj.AsDynamic().WORKACTDETAILCOUNT = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM1 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM2 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM3 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM4 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM5 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM6 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM7 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM8 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM9 = TestDouble;
            obj.AsDynamic().WORKACTDETAILSUM10 = TestDouble;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKACTDETAILID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillWorkActDetail obj)
        {
            obj.AsDynamic().WORKACTDETAILCAUSE10 = TestString;
        }

        protected override void CheckSimpleChange(BillWorkActDetail source, BillWorkActDetail dest)
        {
            string sourceName = source.AsDynamic().WORKACTDETAILCAUSE10;
            string destName = dest.AsDynamic().WORKACTDETAILCAUSE10;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}