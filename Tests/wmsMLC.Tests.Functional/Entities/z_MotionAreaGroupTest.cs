using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MotionAreaGroupTest : BaseWMSObjectTest<MotionAreaGroup>
    {
        protected override void FillRequiredFields(MotionAreaGroup obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MOTIONAREAGROUPCODE = TestString;
            obj.AsDynamic().MOTIONAREAGROUPNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MOTIONAREAGROUPCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MotionAreaGroup obj)
        {
            obj.AsDynamic().MOTIONAREAGROUPDESC = TestString;
        }

        protected override void CheckSimpleChange(MotionAreaGroup source, MotionAreaGroup dest)
        {
            string sourceName = source.AsDynamic().MOTIONAREAGROUPDESC;
            string destName = dest.AsDynamic().MOTIONAREAGROUPDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}