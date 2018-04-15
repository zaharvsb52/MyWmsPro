using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class AdjustmentReasonTest : BaseWMSObjectTest<AdjustmentReason>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(AdjustmentReason obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().ADJUSTMENTREASONID = TestDecimal;
            obj.AsDynamic().ADJUSTMENTREASONCODE = TestString;
            obj.AsDynamic().MANDANTID = _mandantTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ADJUSTMENTREASONID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(AdjustmentReason obj)
        {
            obj.AsDynamic().ADJUSTMENTREASONDESC = TestString;
        }

        protected override void CheckSimpleChange(AdjustmentReason source, AdjustmentReason dest)
        {
            string sourceName = source.AsDynamic().ADJUSTMENTREASONDESC;
            string destName = dest.AsDynamic().ADJUSTMENTREASONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}