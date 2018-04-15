using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Dashboard2UserTest : BaseEntityTest<Dashboard2User>
    {
        protected override void FillRequiredFields(Dashboard2User entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.AsDynamic().DASHBOARD2USERDASHBOARDCODE = DashboardTest.ExistsItem1Code;
            obj.AsDynamic().DASHBOARD2USERUSERCODE = UserTest.ExistsItem1Code;
        }
    }
}