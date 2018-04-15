using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MIUseTest : BaseEntityTest<MIUse>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MIUseFilter";
        }

        protected override void FillRequiredFields(MIUse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MICode_r = MiTest.ExistsItem1Code;
            obj.MIUseStrategyType = TestString;
            obj.ObjectEntityCode_r = SysObjectTest.ExistsItemEntityCode;
            obj.ObjectName_r = SysObjectTest.ExistsItemEntityCode;
        }
    }
}