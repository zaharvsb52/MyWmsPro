using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MeasureTypeTest : BaseWMSObjectTest<MeasureType>
    {
        protected override void FillRequiredFields(MeasureType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MEASURETYPECODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MEASURETYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(MeasureType obj)
        {
            obj.AsDynamic().MEASURETYPENAME = TestString;
        }

        protected override void CheckSimpleChange(MeasureType source, MeasureType dest)
        {
            string sourceName = source.AsDynamic().MEASURETYPENAME;
            string destName = dest.AsDynamic().MEASURETYPENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}