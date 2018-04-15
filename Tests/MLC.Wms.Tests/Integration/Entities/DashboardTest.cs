using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class DashboardTest : BaseEntityTest<Dashboard>
    {
        public const string ExistsItem1Code = "TST_DASHBOARD_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "DASHBOARDDESC";
        }

        protected override void FillRequiredFields(Dashboard entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.DASHBOARDNAME = TestString;
            obj.DASHBOARDVERSION = TestString;
            obj.DASHBOARDBODY = TestString;
        }
    }
}