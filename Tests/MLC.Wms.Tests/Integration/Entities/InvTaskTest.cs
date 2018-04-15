using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvTaskTest : BaseEntityTest<InvTask>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "InvTaskColor";
        }

        protected override void FillRequiredFields(InvTask entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.InvTaskGroupID_r = InvTaskGroupTest.ExistsItem1Id;
            obj.InvTaskStepID_r = InvTaskStepTest.ExistsItem1Id;
            obj.InvTaskNumber = TestDecimal;
            obj.InvTaskManual = true;
            obj.PlaceCode_r = PlaceTest.ExistsItem1Code;
            obj.SKUID_r = SKUTest.ExistsItem1Id;
            obj.InvTaskCount2SKU = TestDecimal;

        }
    }
}