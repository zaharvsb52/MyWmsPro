using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class LabelTest : BaseWMSObjectTest<Label>
    {
        private readonly MandantTest _mandantTestTest = new MandantTest();
        private readonly ReportTest _reportTest = new ReportTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTestTest, _reportTest };
        }

        protected override void FillRequiredFields(Label obj)
        {
            base.FillRequiredFields(obj);

            var report = _reportTest.CreateNew();
            var mandant = _mandantTestTest.CreateNew();

            obj.SetProperty(obj.GetPrimaryKeyPropertyName(), TestString);
            obj.AsDynamic().REPORT_R = report.GetKey();
            obj.AsDynamic().MANDANTID = mandant.GetKey();
            obj.AsDynamic().LABELNAME = Guid.NewGuid().ToString();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(LABELCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Label obj)
        {
            obj.AsDynamic().LABELNAME = Guid.NewGuid().ToString();
        }

        protected override void CheckSimpleChange(Label source, Label dest)
        {
            string sourceName = source.AsDynamic().LABELNAME;
            string destName = dest.AsDynamic().LABELNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
