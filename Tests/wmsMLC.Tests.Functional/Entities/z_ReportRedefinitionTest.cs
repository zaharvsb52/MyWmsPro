using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture, Ignore("Нет атрибута HOST_R")]
    class ReportRedefinitionTest : BaseWMSObjectTest<ReportRedefinition>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("((REPORT_R='{0}' AND REPORTREDEFINITION='{0}' AND HOST_R='{0}'))", TestString);
        }

        protected override void FillRequiredFields(ReportRedefinition obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().REPORT_R = TestString;
            obj.AsDynamic().HOST_R = TestString;
            obj.AsDynamic().REPORTREDEFINITIONREPORT = TestString;
            obj.AsDynamic().REPORTREDEFINITIONCOPIES = TestDecimal;
            obj.AsDynamic().REPORTREDEFINITIONLOCKED = true;
        }

        protected override void MakeSimpleChange(ReportRedefinition obj)
        {
            obj.AsDynamic().LOGICALPRINTER_R = TestString;
        }

        protected override void CheckSimpleChange(ReportRedefinition source, ReportRedefinition dest)
        {
            string sourceName = source.AsDynamic().LOGICALPRINTER_R;
            string destName = dest.AsDynamic().LOGICALPRINTER_R;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
