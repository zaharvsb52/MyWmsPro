using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class DashboardTest : BaseWMSObjectTest<Dashboard>
    {

        protected override void FillRequiredFields(Dashboard obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().DASHBOARDCODE = TestString;
            obj.AsDynamic().DASHBOARDNAME = TestString;
            obj.AsDynamic().DASHBOARDVERSION = TestString;
            obj.AsDynamic().DASHBOARDBODY = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(DASHBOARDCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Dashboard obj)
        {
            obj.AsDynamic().DASHBOARDDESC = TestString;
        }

        protected override void CheckSimpleChange(Dashboard source, Dashboard dest)
        {
            string sourceName = source.AsDynamic().DASHBOARDDESC;
            string destName = dest.AsDynamic().DASHBOARDDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}