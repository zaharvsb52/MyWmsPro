using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TETest : BaseEntityTest<TE>
    {
        public const string ExistsItem1Code = "TST_TE_1";
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEHOSTREF";
        }
        protected override void FillRequiredFields(TE entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TECode = TestString;
            obj.TETypeCode_r = TETypeTest.ExistsItem1Code;
            obj.TECURRENTPLACE = PlaceTest.ExistsItem1Code;
            obj.TECREATIONPLACE = PlaceTest.ExistsItem1Code;
            obj.TELength = TestDecimal;
            obj.TEWidth = TestDecimal;
            obj.TEHeight = TestDecimal;
            obj.TEMaxWeight = TestDecimal;
        }
    }
}