using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ReportFileTest : BaseEntityTest<ReportFile>
    {
        public const string ExistsItem1Code = "TST_REPORTFILE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = ReportFile.ReportFileSubfolderPropertyName;
        }

        protected override void FillRequiredFields(ReportFile entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.REPORTFILEFILE = TestString;
        }
    }
}