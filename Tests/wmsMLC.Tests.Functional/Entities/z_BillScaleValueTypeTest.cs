using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillScaleValueTypeTest : BaseWMSObjectTest<BillScaleValueType>
    {
        protected override void FillRequiredFields(BillScaleValueType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().SCALEVALUETYPECODE = TestString;
            obj.AsDynamic().SCALEVALUETYPENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SCALEVALUETYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillScaleValueType obj)
        {
            obj.AsDynamic().SCALEVALUETYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(BillScaleValueType source, BillScaleValueType dest)
        {
            string sourceName = source.AsDynamic().SCALEVALUETYPEDESC;
            string destName = dest.AsDynamic().SCALEVALUETYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}