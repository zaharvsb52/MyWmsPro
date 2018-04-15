using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillAnalyticsTest : BaseWMSObjectTest<BillAnalytics>
    {

        protected override void FillRequiredFields(BillAnalytics obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ANALYTICSCODE = TestString;
            obj.AsDynamic().ANALYTICSNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ANALYTICSCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillAnalytics obj)
        {
            obj.AsDynamic().ANALYTICSDESC = TestString;
        }

        protected override void CheckSimpleChange(BillAnalytics source, BillAnalytics dest)
        {
            string sourceName = source.AsDynamic().ANALYTICSDESC;
            string destName = dest.AsDynamic().ANALYTICSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}