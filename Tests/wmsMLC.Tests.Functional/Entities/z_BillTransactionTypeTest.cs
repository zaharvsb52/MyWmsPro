using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillTransactionTypeTest : BaseWMSObjectTest<BillTransactionType>
    {
        protected override void FillRequiredFields(BillTransactionType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().TRANSACTIONTYPECODE = TestString;
            obj.AsDynamic().TRANSACTIONTYPENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TRANSACTIONTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillTransactionType obj)
        {
            obj.AsDynamic().TRANSACTIONTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(BillTransactionType source, BillTransactionType dest)
        {
            string sourceName = source.AsDynamic().TRANSACTIONTYPEDESC;
            string destName = dest.AsDynamic().TRANSACTIONTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}