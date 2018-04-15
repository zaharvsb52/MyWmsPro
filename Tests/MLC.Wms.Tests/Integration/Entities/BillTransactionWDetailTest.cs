using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillTransactionWDetailTest : BaseEntityTest<BillTransactionWDetail>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TRANSACTIONWDETAILNAME";
        }

        protected override void FillRequiredFields(BillTransactionWDetail entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TRANSACTIONWID_R = BillTransactionWTest.ExistsItem1Id;
            obj.TRANSACTIONWDETAILNAME = TestString;
        }
    }
}