using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ReportTest : BaseEntityTest<Report>
    {
        public const string ExistsItem1Code = "TST_REPORT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Report.ReportNamePropertyName;
        }

        protected override void FillRequiredFields(Report entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.REPORTFILE_R = ReportFileTest.ExistsItem1Code;
            obj.EPSHANDLER = TestDecimal;
            obj.REPORTCOPIES = TestDecimal;
            obj.REPORTNAME = TestString;
            obj.REPORTTYPE = TestString;
        }
    }
}