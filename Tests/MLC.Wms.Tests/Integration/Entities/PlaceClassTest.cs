using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PlaceClassTest : BaseEntityTest<PlaceClass>
    {
        public const string ExistsItem1Code = "TST_PLACECLASS_1";
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PLACECLASSDESC";
        }

        protected override void FillRequiredFields(PlaceClass entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PlaceClassName = TestString;
        }
    }
}