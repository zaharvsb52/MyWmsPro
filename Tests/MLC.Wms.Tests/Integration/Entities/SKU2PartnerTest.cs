using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SKU2PartnerTest : BaseEntityTest<SKU2Partner>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SKU2PARTNERARTDESC";
        }

        protected override void FillRequiredFields(SKU2Partner entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SKU2PARTNERSKUID = SKUTest.ExistsItem1Id;
            obj.SKU2PARTNERPARTNERID = TstMandantId;
        }
    }
}