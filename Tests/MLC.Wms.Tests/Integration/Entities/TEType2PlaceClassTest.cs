using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TEType2PlaceClassTest : BaseEntityTest<TEType2PlaceClass>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TETYPE2PLACECLASSDESC";
        }

        protected override void FillRequiredFields(TEType2PlaceClass entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TETYPE2PLACECLASSTETYPECODE = TETypeTest.ExistsItem1Code;
            obj.TETYPE2PLACECLASSPLACECLASSCODE = PlaceClassTest.ExistsItem1Code;
        }
    }
}