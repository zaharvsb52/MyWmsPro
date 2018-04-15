using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillTransactionWDetailTest : BaseWMSObjectTest<BillTransactionWDetail>
    {
        private readonly BillTransactionWTest _billTransactionWTest = new BillTransactionWTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billTransactionWTest };
        }

        protected override void FillRequiredFields(BillTransactionWDetail obj)
        {
            base.FillRequiredFields(obj);

            var transact = _billTransactionWTest.CreateNew();

            obj.AsDynamic().TRANSACTIONWDETAILID = TestDecimal;
            obj.AsDynamic().TRANSACTIONWDETAILNAME = TestString;
            obj.AsDynamic().TRANSACTIONWID_R = transact.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSACTIONWDETAILID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillTransactionWDetail obj)
        {
            obj.AsDynamic().TRANSACTIONWDETAILVALUE = TestString;
        }

        protected override void CheckSimpleChange(BillTransactionWDetail source, BillTransactionWDetail dest)
        {
            string sourceName = source.AsDynamic().TRANSACTIONWDETAILVALUE;
            string destName = dest.AsDynamic().TRANSACTIONWDETAILVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}