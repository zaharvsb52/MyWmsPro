using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class SupplyAreaTest : BaseWMSObjectTest<SupplyArea>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("(SUPPLYAREACODE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(SupplyArea obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().SUPPLYAREACODE = TestString;
            obj.AsDynamic().SUPPLYAREANAME = TestString;
        }

        protected override void MakeSimpleChange(SupplyArea obj)
        {
            obj.AsDynamic().SUPPLYAREADESC = TestString;
        }

        protected override void CheckSimpleChange(SupplyArea source, SupplyArea dest)
        {
            string sourceName = source.AsDynamic().SUPPLYAREADESC;
            string destName = dest.AsDynamic().SUPPLYAREADESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}