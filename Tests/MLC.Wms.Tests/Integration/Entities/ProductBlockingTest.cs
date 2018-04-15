using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ProductBlockingTest : BaseEntityTest<ProductBlocking>
    {
        public const string ExistsItem1Code = "TST_PRODUCTBLOCKING_1";
        public const string ExistsItem2Code = "TST_PRODUCTBLOCKING_2";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "BLOCKINGDESC";
        }

        protected override void FillRequiredFields(ProductBlocking entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BLOCKINGNAME = TestString;
        }
    }
}