using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PlaceTypeTest : BaseWMSObjectTest<PlaceType>
    {
        protected override void FillRequiredFields(PlaceType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PLACETYPECODE = TestString;
            obj.AsDynamic().PLACETYPENAME = TestString;
            obj.AsDynamic().PLACETYPECAPACITY = TestDecimal;
            obj.AsDynamic().PLACETYPELENGTH = TestDecimal;
            obj.AsDynamic().PLACETYPEWIDTH = TestDecimal;
            obj.AsDynamic().PLACETYPEHEIGHT = TestDecimal;
            obj.AsDynamic().PLACETYPEMAXWEIGHT = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PLACETYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(PlaceType obj)
        {
            obj.AsDynamic().PLACETYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(PlaceType source, PlaceType dest)
        {
            string sourceName = source.AsDynamic().PLACETYPEDESC;
            string destName = dest.AsDynamic().PLACETYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}