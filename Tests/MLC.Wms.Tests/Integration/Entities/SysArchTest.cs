using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SysArchTest : BaseEntityTest<SysArch>
    {
        public const string ExistsItem1Code = "TST_SYSARCH_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ARCHNAME";
        }

        protected override void FillRequiredFields(SysArch entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ARCHCODE = TestString;
            obj.ARCHNAME = TestString;
            obj.ARCHORDER = TestDecimal;
            obj.ARCHTYPE = SysEnumTest.ExistsEnumValue1;
        }
    }
}
