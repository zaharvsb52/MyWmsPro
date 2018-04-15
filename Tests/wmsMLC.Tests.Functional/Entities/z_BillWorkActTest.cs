using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillWorkActTest : BaseWMSObjectTest<BillWorkAct>
    {
        private readonly BillContractTest _billContractTest = new BillContractTest();
        private readonly BillBillerTest _billBillerTest = new BillBillerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billContractTest, _billBillerTest };
        }

        protected override void FillRequiredFields(BillWorkAct obj)
        {
            base.FillRequiredFields(obj);

            var contract = _billContractTest.CreateNew();
            var biller = _billBillerTest.CreateNew();

            obj.AsDynamic().WORKACTID = TestDecimal;
            obj.AsDynamic().CONTRACTID_R = contract.GetKey();
            obj.AsDynamic().WORKACTDATEFROM = DateTime.Now;
            obj.AsDynamic().WORKACTDATETILL = DateTime.Now;
            obj.AsDynamic().WORKACTDATE = DateTime.Now;
            obj.AsDynamic().WORKACTFIXDATE = DateTime.Now;
            obj.AsDynamic().WORKACTPOSTINGDATE = DateTime.Now;
            obj.AsDynamic().BILLERCODE_R = biller.GetKey();
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKACTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillWorkAct obj)
        {
            obj.AsDynamic().WORKACTHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(BillWorkAct source, BillWorkAct dest)
        {
            string sourceName = source.AsDynamic().WORKACTHOSTREF;
            string destName = dest.AsDynamic().WORKACTHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}