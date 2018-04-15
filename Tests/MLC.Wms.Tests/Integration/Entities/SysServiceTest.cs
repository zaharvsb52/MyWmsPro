using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SysServiceTest : BaseEntityTest<SysService>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SERVICEGROUP";
        }

        protected override void FillRequiredFields(SysService entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SERVICECODE = TestString;
            obj.SERVICETYPE = SysArchTest.ExistsItem1Code;
            obj.SERVICEDEFAULT = 0;
            obj.SERVICEPRIORITY = TestDecimal;
            obj.SERVICELOCKED = 0;
        }
    }
}
