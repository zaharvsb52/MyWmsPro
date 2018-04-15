using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class KitPosTest : BaseEntityTest<KitPos>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = KitPos.KitPosPriorityPropertyName;
        }

        protected override void FillRequiredFields(KitPos entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.KITCODE_R = KitTest.ExistsItem1Code;
            obj.SKUID_R = SKUTest.ExistsItem1Id;
            obj.KITPOSCOUNT = TestDecimal;
        }
    }
}