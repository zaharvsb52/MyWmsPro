using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class SegmentTest : BaseEntityTest<Segment>
    {
        public const string ExistsItem1Code = "TST_SEGMENT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SEGMENTDESC";
        }

        protected override void FillRequiredFields(Segment entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SegmentNumber = TestString;
            obj.AreaCode_r = AreaTest.ExistsItem1Code;
            obj.SegmentTypeCode_r = SegmentTypeTest.ExistsItem1Code;
            obj.SegmentName = TestString;
        }
    }
}