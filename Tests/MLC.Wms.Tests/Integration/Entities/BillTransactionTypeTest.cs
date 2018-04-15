using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillTransactionTypeTest : BaseEntityTest<BillTransactionType>
    {
        public const string ExistsItem1Code = "TST_BILLTRANSACTIONTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TRANSACTIONTYPENAME";
        }

        protected override void FillRequiredFields(BillTransactionType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TRANSACTIONTYPENAME = TestString;
        }
    }
}