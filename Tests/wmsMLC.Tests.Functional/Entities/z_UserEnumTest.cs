using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class UserEnumTest : BaseWMSObjectTest<UserEnum>
    {
        protected override void FillRequiredFields(UserEnum obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().USERENUMID = TestDecimal;
            obj.AsDynamic().USERENUMDESC = TestString;
            obj.AsDynamic().USERENUMGROUP = TestString;
            obj.AsDynamic().USERENUMKEY = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(USERENUMID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(UserEnum obj)
        {
            obj.AsDynamic().USERENUMDESC = TestString + TestString;
        }

        protected override void CheckSimpleChange(UserEnum source, UserEnum dest)
        {
            string sourceName = source.AsDynamic().USERENUMDESC;
            string destName = dest.AsDynamic().USERENUMDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}