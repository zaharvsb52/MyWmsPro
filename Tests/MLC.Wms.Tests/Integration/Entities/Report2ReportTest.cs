using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Report2ReportTest : BaseEntityTest<Report2Report>
    {
        protected override void FillRequiredFields(Report2Report entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.R2RPARENT = ReportTest.ExistsItem1Code;
            obj.REPORT2REPORTREPORT = ReportTest.ExistsItem1Code;
            obj.R2RPRIORITY = TestDecimal;
        }
    }
}