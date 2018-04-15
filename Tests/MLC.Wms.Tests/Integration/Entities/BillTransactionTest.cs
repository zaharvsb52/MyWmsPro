using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillTransactionTest : BaseEntityTest<BillTransaction>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TRANSACTIONAMMOUNT";
        }

        protected override void FillRequiredFields(BillTransaction entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLERCODE_R = BillBillerTest.ExistsItem1Code;
            obj.EVENTHEADERID_R = EventHeaderTest.ExistsItem1Id;
            obj.TRANSACTIONTYPECODE_R = BillTransactionTypeTest.ExistsItem1Code;
            obj.MANDANTID = TstMandantId;
            obj.TRANSACTIONRECIPIENT = TstMandantId;
            obj.TRANSACTIONAMMOUNT = TestDouble;
            obj.CURRENCYCODE_R = IsoCurrencyTest.ExistsItem1Code;
        }
    }
}