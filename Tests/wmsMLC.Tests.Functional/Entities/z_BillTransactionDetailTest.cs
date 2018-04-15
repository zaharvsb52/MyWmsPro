using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillTransactionDetailTest : BaseWMSObjectTest<BillTransactionDetail>
    {
        private readonly BillTransactionTest _billTransactionTest = new BillTransactionTest();

        public BillTransactionDetailTest()
        {
            TestDecimal = TestDecimal*(-1);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billTransactionTest };
        }

        protected override void FillRequiredFields(BillTransactionDetail obj)
        {
            base.FillRequiredFields(obj);

            var transaction = _billTransactionTest.CreateNew();

            obj.AsDynamic().TRANSACTIONDETAILID = TestDecimal;
            obj.AsDynamic().TRANSACTIONID_R = transaction.GetKey();
            obj.AsDynamic().TRANSACTIONDETAILNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSACTIONDETAILID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BillTransactionDetail obj)
        {
            obj.AsDynamic().TRANSACTIONDETAILVALUE = TestString;
        }

        protected override void CheckSimpleChange(BillTransactionDetail source, BillTransactionDetail dest)
        {
            string sourceName = source.AsDynamic().TRANSACTIONDETAILVALUE;
            string destName = dest.AsDynamic().TRANSACTIONDETAILVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}