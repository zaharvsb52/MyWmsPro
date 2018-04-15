using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Report2ReportTest : BaseWMSObjectTest<Report2Report>
    {
        private readonly ReportTest _reportTest1 = new ReportTest();
        
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _reportTest1 };
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(R2RID = '{0}')", TestDecimal);
        }

        protected override void FillRequiredFields(Report2Report obj)
        {
            base.FillRequiredFields(obj);

            var report1 = _reportTest1.CreateNew();

            obj.AsDynamic().R2RID = TestDecimal;
            obj.AsDynamic().R2RPARENT = report1.GetKey();
            obj.AsDynamic().R2RPRIORITY = TestDecimal;
            obj.AsDynamic().REPORT2REPORTREPORT = report1.GetKey();
        }
    }
}