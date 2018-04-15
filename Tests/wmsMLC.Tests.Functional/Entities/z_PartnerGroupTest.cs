using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PartnerGroupTest : BaseWMSObjectTest<PartnerGroup>
    {
        protected override void FillRequiredFields(PartnerGroup obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PARTNERGROUPID = TestDecimal;
            obj.AsDynamic().PARTNERGROUPNAME = TestString;
            obj.AsDynamic().PARTNERGROUPHOSTREF = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PARTNERGROUPID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PartnerGroup obj)
        {
            obj.AsDynamic().PARTNERGROUPDESC = TestString;
        }

        protected override void CheckSimpleChange(PartnerGroup source, PartnerGroup dest)
        {
            string sourceName = source.AsDynamic().PARTNERGROUPDESC;
            string destName = dest.AsDynamic().PARTNERGROUPDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}