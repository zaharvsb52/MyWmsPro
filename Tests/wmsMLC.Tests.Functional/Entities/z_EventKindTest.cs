using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class EventKindTest : BaseWMSObjectTest<EventKind>
    {
        protected override void FillRequiredFields(EventKind obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().EVENTKINDCODE = TestString;
            obj.AsDynamic().EVENTKINDNAME = TestString;
            obj.AsDynamic().EVENTKINDDESC = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENTKINDCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(EventKind obj)
        {
            obj.AsDynamic().EVENTKINDDESC = TestString;
        }

        protected override void CheckSimpleChange(EventKind source, EventKind dest)
        {
            string sourceName = source.AsDynamic().EVENTKINDDESC;
            string destName = dest.AsDynamic().EVENTKINDDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}