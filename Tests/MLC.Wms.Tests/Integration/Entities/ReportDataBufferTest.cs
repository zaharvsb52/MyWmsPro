using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ReportDataBufferTest : BaseEntityTest<ReportDataBuffer>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ReportDataBufferValue";
        }

        protected override void FillRequiredFields(ReportDataBuffer entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ReportDataBufferGroup = TestGuid;
            obj.ReportDataBufferRecord = TestGuid;
            obj.ReportDataBufferKey = TestString;
        }
    }
}