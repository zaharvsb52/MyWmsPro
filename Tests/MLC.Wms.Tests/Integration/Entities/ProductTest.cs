using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ProductTest : BaseEntityTest<Product>
    {
        public const int ExistsItem1Id = -1;
        public const int ExistsItem2Id = -2;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Product.PRODUCTLOTPropertyName;
        }

        protected override void FillRequiredFields(Product entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TECode_r = TETest.ExistsItem1Code;
            obj.SKUID_r = SKUTest.ExistsItem1Id;
            obj.ProductCountSKU = TestDecimal;
            obj.ProductCount = TestDecimal;
            obj.ProductTTEQuantity = TestDecimal;
            obj.QLFCode_r = QlfTest.ExistsItem1Code;
            obj.ProductInputDate = TestDateTime;
            obj.ProductInputDateMethod = "DAY";
            obj.ArtCode_r = ArtTest.ExistsItem1Code;
            obj.MandantID = PartnerTest.ExistsItem1Id;
            obj.ProductOwner = PartnerTest.ExistsItem1Id;
        }
    }
}