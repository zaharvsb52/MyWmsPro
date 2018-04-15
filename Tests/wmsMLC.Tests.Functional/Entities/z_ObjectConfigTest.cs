using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ObjectConfigTest : BaseWMSObjectTest<ObjectConfig>
    {
        protected override void FillRequiredFields(ObjectConfig obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().OBJECTCONFIGCODE = TestString;
            obj.AsDynamic().OBJECTCONFIGNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OBJECTCONFIGCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(ObjectConfig obj)
        {
            obj.AsDynamic().OBJECTCONFIGDESC = TestString;
        }

        protected override void CheckSimpleChange(ObjectConfig source, ObjectConfig dest)
        {
            string sourceName = source.AsDynamic().OBJECTCONFIGDESC;
            string destName = dest.AsDynamic().OBJECTCONFIGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}