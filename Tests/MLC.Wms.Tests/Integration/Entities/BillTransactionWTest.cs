using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillTransactionWTest : BaseEntityTest<BillTransactionW>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TRANSACTIONWAMMOUNT";
        }

        protected override void FillRequiredFields(BillTransactionW entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.EVENTHEADERID_R = EventHeaderTest.ExistsItem1Id;
            obj.BILLERCODE_R = BillBillerTest.ExistsItem1Code;
            obj.TRANSACTIONTYPECODE_R = BillTransactionTypeTest.ExistsItem1Code;
            obj.MANDANTID = TstMandantId;
            obj.WORKERID_R = WorkerTest.ExistsItem1Id;
            obj.TRANSACTIONWAMMOUNT = TestDouble;
            obj.CURRENCYCODE_R = IsoCurrencyTest.ExistsItem1Code;
        }
    }
}