using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillAnalyticsTest : BaseEntityTest<BillAnalytics>
    {
        public const string ExistsItem1Code = "TST_BILLANALYTICS_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ANALYTICSDESC";
        }

        protected override void FillRequiredFields(BillAnalytics entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ANALYTICSNAME = TestString;
        }
    }
}