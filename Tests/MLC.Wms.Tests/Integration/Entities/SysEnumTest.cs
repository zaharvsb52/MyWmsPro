using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SysEnumTest : BaseEntityTest<SysEnum>
    {
        public const decimal ExistsItem1Id = -1;
        public const string ExistsEnumValue1 = "TST_SYSENUM_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = SysEnum.SysEnumNamePropertyName;
        }

        protected override void FillRequiredFields(SysEnum entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ENUMGROUP = TestString;
            obj.ENUMKEY = TestString;
        }
    }
}