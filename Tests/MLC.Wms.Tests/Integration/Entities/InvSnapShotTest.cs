using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvSnapShotTest : BaseEntityTest<InvSnapShot>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "InvSSLot";
        }

        protected override void FillRequiredFields(InvSnapShot entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.InvID_r = InvTest.ExistsItem1Id;
            obj.InvGroupID_r = InvGroupTest.ExistsItem1Id;
            obj.PlaceCode_r = PlaceTest.ExistsItem1Code;
            obj.SKUID_r = SKUTest.ExistsItem1Id;
            obj.InvSSCount = TestDecimal;
            obj.InvSSCount2SKU = TestDecimal;
        }
    }
}