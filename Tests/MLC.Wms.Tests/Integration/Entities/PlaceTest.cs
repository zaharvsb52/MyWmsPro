using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PlaceTest : BaseEntityTest<Place>
    {
        public const string ExistsItem1Code = "TST_PLACE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PLACEHOSTREF";
        }

        protected override void FillRequiredFields(Place entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SegmentCode_r = SegmentTest.ExistsItem1Code;
            obj.PlaceS = TestDecimal;
            obj.PlaceX = TestDecimal;
            obj.PlaceY = TestDecimal;
            obj.PlaceZ = TestDecimal;
            obj.PlaceCapacityMax = TestDecimal;
            obj.PlaceCapacity = TestDecimal;
            obj.PlaceTypeCode_r = PlaceTypeTest.ExistsItem1Code;
            obj.PlaceClassCode_r = PlaceClassTest.ExistsItem1Code;
            obj.PlaceSortA = TestDecimal;
            obj.PlaceSortB = TestDecimal;
            obj.PlaceSortC = TestDecimal;
            obj.PlaceSortD = TestDecimal;
            obj.PlaceSortPick = TestDecimal;
            obj.PlaceWeight = TestDecimal;
            obj.PlaceWeightGroup = TestDecimal;
        }
    }
}