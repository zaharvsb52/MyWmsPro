using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SKU2TTETest : BaseEntityTest<SKU2TTE>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SKU2TTECRITMSC";
        }

        protected override void FillRequiredFields(SKU2TTE entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SKU2TTETETYPECODE = TETypeTest.ExistsItem1Code;
            obj.SKU2TTESKUID = SKUTest.ExistsItem1Id;
            obj.SKU2TTEDEFAULT = TestBool;
            obj.SKU2TTEQUANTITY = TestDecimal;
            obj.SKU2TTEQUANTITYMAX = TestDecimal;
            obj.SKU2TTEMAXWEIGHT = TestDecimal;
        }
    }
}