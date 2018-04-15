using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PLPosTest : BaseEntityTest<PLPos>
    {
        protected override void FillRequiredFields(PLPos entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PLID_r = PLTest.ExistsItem1Id;
            obj.PlaceCode_r = PlaceTest.ExistsItem1Code;
            obj.TECode_r = TETest.ExistsItem1Code;
            obj.PLPosSort = TestDecimal;
            obj.SKUID_r = SKUTest.ExistsItem1Id;
            obj.PLPosCountSKUPlan = TestDecimal;
            obj.PLPosCountSKUFact = TestDecimal;
        }
    }
}