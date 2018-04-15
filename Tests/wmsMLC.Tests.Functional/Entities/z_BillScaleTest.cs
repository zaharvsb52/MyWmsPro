using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillScaleTest : BaseWMSObjectTest<BillScale>
    {
        protected override void FillRequiredFields(BillScale obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().SCALECODE = TestString;
            obj.AsDynamic().SCALENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SCALECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillScale obj)
        {
            obj.AsDynamic().SCALEDESCRIPTION = TestString;
        }

        protected override void CheckSimpleChange(BillScale source, BillScale dest)
        {
            string sourceName = source.AsDynamic().SCALEDESCRIPTION;
            string destName = dest.AsDynamic().SCALEDESCRIPTION;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}