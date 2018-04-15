using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Product2BlockingTest : BaseEntityTest<Product2Blocking>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PRODUCT2BLOCKINGDESC";
        }

        protected override void FillRequiredFields(Product2Blocking entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PRODUCT2BLOCKINGPRODUCTID = ProductTest.ExistsItem1Id;
            obj.PRODUCT2BLOCKINGBLOCKINGCODE = ProductBlockingTest.ExistsItem1Code;
        }
    }
}