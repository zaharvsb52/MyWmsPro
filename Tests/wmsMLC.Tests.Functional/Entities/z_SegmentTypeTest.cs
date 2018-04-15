using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class SegmentTypeTest : BaseWMSObjectTest<SegmentType>
    {
        protected override void FillRequiredFields(SegmentType obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().SEGMENTTYPECODE = TestString;
            obj.AsDynamic().SEGMENTTYPENAME = TestString;
            obj.AsDynamic().SEGMENTTYPECODEFORMAT = TestString;
            obj.AsDynamic().SEGMENTTYPECODEVIEW = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SEGMENTTYPECODE = '{0}')", TestString);
        }
        
        protected override void MakeSimpleChange(SegmentType obj)
        {
            obj.AsDynamic().SEGMENTTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(SegmentType source, SegmentType dest)
        {
            string sourceName = source.AsDynamic().SEGMENTTYPEDESC;
            string destName = dest.AsDynamic().SEGMENTTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}