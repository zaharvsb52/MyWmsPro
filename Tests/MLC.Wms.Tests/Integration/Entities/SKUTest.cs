using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SKUTest : BaseEntityTest<SKU>
    {
        public const int ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SKUDESC";
        }

        protected override void FillRequiredFields(SKU entity)
        {
            base.FillRequiredFields(entity);

            entity.SKUPrimary = false;
            entity.SKUParent = ExistsItem1Id;
            entity.ArtCode = ArtTest.ExistsItem1Code;
            entity.MeasureCode = MeasureTest.ExistsItem2Code;
            entity.SKUCount = TestDouble;
            entity.SKUName = TestString;
        }
    }
}