using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SysEnumTest : BaseWMSObjectTest<SysEnum>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("((ENUMGROUP='{0}' OR ENUMKEY='{0}'))", TestString);
        }

        protected override void FillRequaredFields(SysEnum obj)
        {
            base.FillRequaredFields(obj);
            obj.AsDynamic().ENUMGROUP = TestString;
            obj.AsDynamic().ENUMKEY = TestString;
        }

        protected override void MakeSimpleChange(SysEnum obj)
        {
            obj.AsDynamic().ENUMVALUE = TestString;
        }

        protected override void CheckSimpleChange(SysEnum source, SysEnum dest)
        {
            string sourceName = source.AsDynamic().ENUMVALUE;
            string destName = dest.AsDynamic().ENUMVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
