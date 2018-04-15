using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Place2BlockingTest : BaseEntityTest<Place2Blocking>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PLACE2BLOCKINGDESC";
        }

        protected override void FillRequiredFields(Place2Blocking entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PLACE2BLOCKINGPLACECODE = PlaceTest.ExistsItem1Code;
            obj.PLACE2BLOCKINGBLOCKINGCODE = ProductBlockingTest.ExistsItem1Code;
        }
    }
}