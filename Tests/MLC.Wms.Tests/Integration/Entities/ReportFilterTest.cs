using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ReportFilterTest : BaseEntityTest<ReportFilter>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ReportFilterDesc";
        }

        protected override void FillRequiredFields(ReportFilter entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.Report_r = ReportTest.ExistsItem1Code;
            obj.ReportFilterParameter = TestString;
            obj.ReportFilterDataType = 1;
            obj.ReportFilterDisplayName = TestString;
            obj.ReportFilterMethod = TestString;
            obj.ReportFilterVisible = TestBool;

        }
    }
}