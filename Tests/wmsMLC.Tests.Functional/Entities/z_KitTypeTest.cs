using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class KitTypeTest : BaseWMSObjectTest<KitType>
    {
        protected override void FillRequiredFields(KitType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().KITTYPECODE = TestString;
            obj.AsDynamic().KITTYPENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(KITTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(KitType obj)
        {
            obj.AsDynamic().KITTYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(KitType source, KitType dest)
        {
            string sourceName = source.AsDynamic().KITTYPEDESC;
            string destName = dest.AsDynamic().KITTYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}