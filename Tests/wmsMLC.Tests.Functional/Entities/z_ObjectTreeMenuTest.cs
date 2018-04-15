using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ObjectTreeMenuTest : BaseWMSObjectTest<ObjectTreeMenu>
    {
        public const string TestNameSysObject = "SYSTEMTREEMENU";

        protected override void FillRequiredFields(ObjectTreeMenu obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().OBJECTTREECODE = TestString;
            obj.AsDynamic().OBJECTTREENAME = TestString;
            obj.AsDynamic().OBJECTNAME_R = TestNameSysObject;
            obj.AsDynamic().OBJECTTREEMENUTYPE = ObjectTreeMenuType.DCL;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OBJECTTREECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(ObjectTreeMenu obj)
        {
            obj.AsDynamic().OBJECTTREEACTION = TestString;
        }

        protected override void CheckSimpleChange(ObjectTreeMenu source, ObjectTreeMenu dest)
        {
            string sourceName = source.AsDynamic().OBJECTTREEACTION;
            string destName = dest.AsDynamic().OBJECTTREEACTION;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }

    }
}
