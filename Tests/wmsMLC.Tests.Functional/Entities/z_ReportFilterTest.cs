using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ReportFilterTest : BaseWMSObjectTest<ReportFilter>
    {
        private readonly ReportTest _reportTest = new ReportTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _reportTest };
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(REPORTFILTERPARAMETER = '{0}')", TestString);
        }

        protected override void FillRequiredFields(ReportFilter obj)
        {
            base.FillRequiredFields(obj);

            var report = _reportTest.CreateNew();

            obj.AsDynamic().REPORT_R = report.GetKey();
            obj.AsDynamic().REPORTFILTERPARAMETER = TestString;
            obj.AsDynamic().REPORTFILTERDATATYPE = 16;
            obj.AsDynamic().REPORTFILTERDISPLAYNAME = TestString;
            obj.AsDynamic().REPORTFILTERMETHOD = "REPORTFILTER";
            obj.AsDynamic().REPORTFILTERORDER = TestDecimal;
            obj.AsDynamic().REPORTFILTERFORMAT = TestString;
            obj.AsDynamic().OBJECTLOOKUPCODE_R = "PARTNER_PARTNERID";
            obj.AsDynamic().REPORTFILTERDEFAULTVALUE = TestString;
            obj.AsDynamic().REPORTFILTERDESC = obj.AsDynamic().REPORTFILTERDISPLAYNAME + TestString + obj.AsDynamic().REPORTFILTERPARAMETER;
        }

        protected override void MakeSimpleChange(ReportFilter obj)
        {
            obj.AsDynamic().REPORTFILTERDESC = TestString;
        }

        protected override void CheckSimpleChange(ReportFilter source, ReportFilter dest)
        {
            string sourceName = source.AsDynamic().REPORTFILTERDESC;
            string destName = dest.AsDynamic().REPORTFILTERDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
