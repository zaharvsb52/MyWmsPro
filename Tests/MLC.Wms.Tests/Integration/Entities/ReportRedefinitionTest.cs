using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ReportRedefinitionTest : BaseEntityTest<ReportRedefinition>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ReportRedefinitionDesc";
        }

        protected override void FillRequiredFields(ReportRedefinition entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.Report_r = ReportTest.ExistsItem1Code;
            obj.REPORTREDEFINITIONREPORT = ReportTest.ExistsItem1Code;
            obj.REPORTREDEFINITIONCOPIES = TestDecimal;
            obj.REPORTREDEFINITIONLOCKED = TestBool;
            
        }
    }
}