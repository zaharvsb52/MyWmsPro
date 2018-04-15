using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class MotionAreaTest : BaseWMSObjectTest<MotionArea>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("(MOTIONAREACODE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(MotionArea obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MOTIONAREACODE = TestString;
            obj.AsDynamic().MOTIONAREANAME = TestString;
        }

        protected override void MakeSimpleChange(MotionArea obj)
        {
            obj.AsDynamic().MOTIONAREADESC = TestString;
        }

        protected override void CheckSimpleChange(MotionArea source, MotionArea dest)
        {
            string sourceName = source.AsDynamic().MOTIONAREADESC;
            string destName = dest.AsDynamic().MOTIONAREADESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
    
}