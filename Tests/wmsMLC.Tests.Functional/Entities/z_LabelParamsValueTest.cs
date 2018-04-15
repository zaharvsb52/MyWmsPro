using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class LabelParamsValueTest : BaseWMSObjectTest<LabelParamsValue>
    {
        private readonly LabelParamsTest _labelParamsTest = new LabelParamsTest();
        private readonly LabelUseTest _labelUseTest = new LabelUseTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _labelParamsTest, _labelUseTest };
        }

        protected override void FillRequiredFields(LabelParamsValue obj)
        {
            base.FillRequiredFields(obj);

            var labelUse = _labelUseTest.CreateNew();
            _labelParamsTest.TestString = TestString + "1";
            _labelParamsTest.TestDecimal = TestDecimal + 1;
            var labelParams = _labelParamsTest.CreateNew();

            obj.AsDynamic().LABELPARAMSVALUEID = TestDecimal;
            obj.AsDynamic().LABELUSEID_R = labelUse.GetKey();
            obj.AsDynamic().LABELPARAMSID_R = labelParams.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(LABELPARAMSVALUEID = {0})", TestDecimal);
        }

        protected override void MakeSimpleChange(LabelParamsValue obj)
        {
            obj.AsDynamic().LABELPARAMSVALUETEXT = TestString;
        }

        protected override void CheckSimpleChange(LabelParamsValue source, LabelParamsValue dest)
        {
            string sourceName = source.AsDynamic().LABELPARAMSVALUETEXT;
            string destName = dest.AsDynamic().LABELPARAMSVALUETEXT;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
