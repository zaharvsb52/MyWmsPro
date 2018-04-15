using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SegmentTest : BaseWMSObjectTest<Segment>
    {
        private readonly AreaTest _areaTest = new AreaTest();
        private readonly SegmentTypeTest _segmentTypeTest = new SegmentTypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _areaTest, _segmentTypeTest };
        } 

        protected override void FillRequiredFields(Segment obj)
        {
            base.FillRequiredFields(obj);

            _areaTest.TestString = TestString;
            var area = _areaTest.CreateNew();
            _segmentTypeTest.TestString = TestString;
            var segmentType = _segmentTypeTest.CreateNew();

            obj.AsDynamic().SEGMENTCODE = TestString;
            obj.AsDynamic().SEGMENTNUMBER = TestString;
            obj.AsDynamic().SEGMENTNAME = TestString;
            obj.AsDynamic().AREACODE_R = area.GetKey();
            obj.AsDynamic().SEGMENTTYPECODE_R = segmentType.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SEGMENTCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Segment obj)
        {
            obj.AsDynamic().SEGMENTDESC = TestString;
        }

        protected override void CheckSimpleChange(Segment source, Segment dest)
        {
            string sourceName = source.AsDynamic().SEGMENTDESC;
            string destName = dest.AsDynamic().SEGMENTDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }


        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
