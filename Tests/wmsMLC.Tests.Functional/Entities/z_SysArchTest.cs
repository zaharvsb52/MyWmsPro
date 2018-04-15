using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class z_SysArchTest : BaseWMSObjectTest<SysArch>
    {
        protected override void FillRequiredFields(SysArch obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ARCHCODE = TestString;
            obj.AsDynamic().ARCHNAME = TestString;
            obj.AsDynamic().ARCHTYPE = TestString;
            obj.AsDynamic().ARCHBATCH = TestString;
            obj.AsDynamic().ARCHORDER = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ARCHCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(SysArch obj)
        {
            obj.AsDynamic().ARCHDESC = TestString;
        }

        protected override void CheckSimpleChange(SysArch source, SysArch dest)
        {
            string sourceName = source.AsDynamic().ARCHDESC;
            string destName = dest.AsDynamic().ARCHDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}