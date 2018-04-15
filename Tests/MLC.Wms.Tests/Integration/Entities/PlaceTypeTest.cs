using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PlaceTypeTest : BaseEntityTest<PlaceType>
    {
        public const string ExistsItem1Code = "TST_PLACETYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PLACETYPEDESC";
        }

        protected override void FillRequiredFields(PlaceType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PlaceTypeName = TestString;
            obj.PlaceTypeCapacity = TestDecimal;
            obj.PlaceTypeLength = TestDecimal;
            obj.PlaceTypeWidth = TestDecimal;
            obj.PlaceTypeHeight = TestDecimal;
            obj.PlaceTypeMaxWeight = TestDecimal;
        }
    }
}