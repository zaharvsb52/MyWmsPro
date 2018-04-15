using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class LabelParamsTest : BaseWMSObjectTest<LabelParams>
    {
        private readonly LabelTest _labelTest = new LabelTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _labelTest };
        }

        protected override void FillRequiredFields(LabelParams obj)
        {
            base.FillRequiredFields(obj);

            var label = _labelTest.CreateNew();

            obj.SetProperty(obj.GetPrimaryKeyPropertyName(), TestDecimal);
            obj.AsDynamic().LABELCODE_R = label.GetKey();
            obj.AsDynamic().LABELPARAMSNAME = Guid.NewGuid().ToString();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(LABELPARAMSID = {0})", TestDecimal);
        }

        protected override void MakeSimpleChange(LabelParams obj)
        {
            obj.AsDynamic().LABELPARAMSDESC = Guid.NewGuid().ToString();
        }

        protected override void CheckSimpleChange(LabelParams source, LabelParams dest)
        {
            string sourceName = source.AsDynamic().LABELPARAMSNAME;
            string destName = dest.AsDynamic().LABELPARAMSNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
