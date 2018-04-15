using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Segment2BlockingTest : BaseEntityTest<Segment2Blocking>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "Segment2BlockingDesc";
        }
        protected override void FillRequiredFields(Segment2Blocking entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SEGMENT2BLOCKINGSEGMENTCODE = SegmentTest.ExistsItem1Code;
            obj.SEGMENT2BLOCKINGBLOCKINGCODE = ProductBlockingTest.ExistsItem1Code;
        }
    }
}