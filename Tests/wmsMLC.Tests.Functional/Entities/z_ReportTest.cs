using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ReportTest : BaseWMSObjectTest<Report>
    {
        private readonly ReportFileTest _reportFileTest = new ReportFileTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _reportFileTest };
        }
        
        protected override string GetCheckFilter()
        {
            return string.Format("(REPORT = '{0}')", TestString);
        }

        protected override void FillRequiredFields(Report obj)
        {
            base.FillRequiredFields(obj);

            var reportFile = _reportFileTest.CreateNew();

            obj.AsDynamic().REPORTCODE = TestString;
            obj.AsDynamic().REPORTCOPIES = TestDecimal;
            obj.AsDynamic().REPORTNAME = TestString;
            obj.AsDynamic().EPSHANDLER = TestDecimal;
            obj.AsDynamic().REPORTFILE_R = reportFile.AsDynamic().REPORTFILEFILE;
            obj.AsDynamic().REPORTTYPE = TestString;
        }

        protected override void MakeSimpleChange(Report obj)
        {
            obj.AsDynamic().REPORTDESC = TestString;
        }

        protected override void CheckSimpleChange(Report source, Report dest)
        {
            string sourceName = source.AsDynamic().REPORTDESC;
            string destName = dest.AsDynamic().REPORTDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}