using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillEventKind2BillerTest : BaseEntityTest<BillEventKind2Biller>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "EVENTKIND2BILLEREVENTDETAIL";
        }

        protected override void FillRequiredFields(BillEventKind2Biller entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLEVENTKIND2BILLEREVENTKINDCODE = EventKindTest.ExistsItem1Code;
            obj.BILLEVENTKIND2BILLERBILLERCODE = BillBillerTest.ExistsItem1Code;
        }
    }
}