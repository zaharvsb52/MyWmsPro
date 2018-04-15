using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PlaceClassTest : BaseWMSObjectTest<PlaceClass>
    {
       protected override void FillRequiredFields(PlaceClass obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PLACECLASSCODE = TestString;
            obj.AsDynamic().PLACECLASSNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PLACECLASSCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(PlaceClass obj)
        {
            obj.AsDynamic().PLACECLASSDESC = TestString;
        }

        protected override void CheckSimpleChange(PlaceClass source, PlaceClass dest)
        {
            string sourceName = source.AsDynamic().PLACECLASSDESC;
            string destName = dest.AsDynamic().PLACECLASSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}