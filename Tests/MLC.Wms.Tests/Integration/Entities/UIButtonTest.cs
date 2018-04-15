using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class UIButtonTest : BaseEntityTest<UIButton>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "UIButtonDesc";
        }

        protected override void FillRequiredFields(UIButton entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.UIButtonPanel = WorkGroupTest.ExistsItem1Id;
            obj.UIButtonOrder = TestDecimal;
            obj.UIButtonCaption = StatusTest.ExistsItem1Code;
        }
    }
}