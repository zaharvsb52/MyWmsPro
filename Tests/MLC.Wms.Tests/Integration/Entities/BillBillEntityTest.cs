using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillBillEntityTest : BaseEntityTest<BillBillEntity>
    {
        public const string ExistsItem1Code = "TST_BILLBILLENTITY_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "BILLENTITYEVENTFIELD";
        }

        protected override void FillRequiredFields(BillBillEntity entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLENTITYEVENTFIELD = TestString;
        }
    }
}