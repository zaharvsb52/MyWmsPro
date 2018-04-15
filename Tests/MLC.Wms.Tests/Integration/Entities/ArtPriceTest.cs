using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ArtPriceTest : BaseEntityTest<ArtPrice>
    {
        //public const string ExistsItem1Code = "TST_ARTPRICE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = ArtPrice.ArtPriceValuePropertyName;
        }

        protected override void FillRequiredFields(ArtPrice entity)
        {
            base.FillRequiredFields(entity);

            entity.ArtPriceSKUID = SKUTest.ExistsItem1Id;
            entity.ArtPriceValue = TestDouble;

            dynamic obj = entity.AsDynamic();
            obj.ARTPRICEVAT = TestDouble;
        }
    }
}