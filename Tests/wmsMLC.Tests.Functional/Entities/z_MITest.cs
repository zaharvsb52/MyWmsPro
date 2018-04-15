using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MITest : BaseWMSObjectTest<MI>
    {
        protected override void FillRequiredFields(MI obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MICODE = TestGuid;
            obj.AsDynamic().MINAME = TestString;
            obj.AsDynamic().MILINE = TestDecimal;
            obj.AsDynamic().MILINEPERPAGE = TestDecimal;
            obj.AsDynamic().MICALCBAN = true;
            obj.AsDynamic().MICALCTYPE = TestString;
            obj.AsDynamic().MIASKSKU = true;
            obj.AsDynamic().MIINVTYPE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MICODE = '{0}')", TestGuid.ToString("N"));
        }

        protected override void MakeSimpleChange(MI obj)
        {
            obj.AsDynamic().MIDESC = TestString;
        }

        protected override void CheckSimpleChange(MI source, MI dest)
        {
            string sourceName = source.AsDynamic().MIDESC;
            string destName = dest.AsDynamic().MIDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}