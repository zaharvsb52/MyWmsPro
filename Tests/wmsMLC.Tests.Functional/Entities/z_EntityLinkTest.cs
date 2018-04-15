using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class EntityLinkTest : BaseWMSObjectTest<EntityLink>
    {

        protected override void FillRequiredFields(EntityLink obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ENTITYLINKCODE = TestString;
            obj.AsDynamic().ENTITYLINKNAME = TestString;
            obj.AsDynamic().ENTITYLINKFROM = TestString;
            obj.AsDynamic().ENTITYLINKTO = TestString;
            obj.AsDynamic().ENTITYLINKTYPE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ENTITYLINKCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(EntityLink obj)
        {
            obj.AsDynamic().ENTITYLINKDESC = TestString;
        }

        protected override void CheckSimpleChange(EntityLink source, EntityLink dest)
        {
            string sourceName = source.AsDynamic().ENTITYLINKDESC;
            string destName = dest.AsDynamic().ENTITYLINKDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}