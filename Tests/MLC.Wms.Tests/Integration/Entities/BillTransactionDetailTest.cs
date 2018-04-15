using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillTransactionDetailTest : BaseEntityTest<BillTransactionDetail>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TRANSACTIONDETAILNAME";
        }

        protected override void FillRequiredFields(BillTransactionDetail entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TRANSACTIONID_R = BillTransactionTest.ExistsItem1Id;
            obj.TRANSACTIONDETAILNAME = TestString;
        }
    }
}