using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TE2BlockingTest : BaseEntityTest<TE2Blocking>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TE2BLOCKINGDESC";
        }

        protected override void FillRequiredFields(TE2Blocking entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TE2BLOCKINGTECODE = TETest.ExistsItem1Code;
            obj.TE2BLOCKINGBLOCKINGCODE = ProductBlockingTest.ExistsItem1Code;
        }
    }
}