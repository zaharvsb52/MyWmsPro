using FluentAssertions;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    public class WTVTest : BaseWMSObjectTest<WTV>
    {
        protected override void FillRequiredFields(WTV obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().WTVID = TestDecimal;
            obj.AsDynamic().WTVPRODUCTHISTORY = TestDecimal;
            obj.AsDynamic().WTVCOUNTSKU = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WTVID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(WTV obj)
        {
            obj.AsDynamic().WTVCOUNTSKU = TestDecimal + 1;
        }

        protected override void CheckSimpleChange(WTV source, WTV dest)
        {
            decimal sourceName = source.AsDynamic().WTVCOUNTSKU;
            decimal destName = dest.AsDynamic().WTVCOUNTSKU;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}