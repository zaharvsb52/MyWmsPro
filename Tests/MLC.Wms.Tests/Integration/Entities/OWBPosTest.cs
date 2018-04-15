using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class OWBPosTest : BaseEntityTest<OWBPos>
    {
        public const int ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OWBPosColor";
        }

        protected override void FillRequiredFields(OWBPos entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OWBID_r = OwbTest.ExistsItem1Code;
            obj.OWBPosNumber = TestDecimal;
            obj.SKUID_r = SKUTest.ExistsItem1Id;
            obj.OWBPosCount = TestDecimal;
            obj.OWBPosCount2SKU = TestDecimal;
            obj.StatusCode_r = TestString;
            obj.QLFCode_r = QlfTest.ExistsItem1Code;
        }
    }
}