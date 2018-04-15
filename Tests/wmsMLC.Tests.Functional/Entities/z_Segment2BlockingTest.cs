using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Segment2BlockingTest : BaseWMSObjectTest<Segment2Blocking>
    {
        private readonly SegmentTest _segmentTest = new SegmentTest();
        private readonly ProductBlockingTest _productBlockingTest = new ProductBlockingTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _segmentTest, _productBlockingTest };
        }
        
        protected override void FillRequiredFields(Segment2Blocking obj)
        {
            base.FillRequiredFields(obj);

            var seg = _segmentTest.CreateNew();
            var block = _productBlockingTest.CreateNew(bl =>
                {
                    bl.AsDynamic().BLOCKINGFORPLACE = true;
                });

            obj.AsDynamic().SEGMENT2BLOCKINGID = TestDecimal;
            obj.AsDynamic().SEGMENT2BLOCKINGSEGMENTCODE = seg.GetKey();
            obj.AsDynamic().SEGMENT2BLOCKINGBLOCKINGCODE = block.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SEGMENT2BLOCKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Segment2Blocking obj)
        {
            obj.AsDynamic().SEGMENT2BLOCKINGDESC = TestString;
        }

        protected override void CheckSimpleChange(Segment2Blocking source, Segment2Blocking dest)
        {
            string sourceName = source.AsDynamic().SEGMENT2BLOCKINGDESC;
            string destName = dest.AsDynamic().SEGMENT2BLOCKINGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Тест не запускаем. Нет хистори")]
        public override void ManagerGetHistoryTest()
        {

        }

    }
}
