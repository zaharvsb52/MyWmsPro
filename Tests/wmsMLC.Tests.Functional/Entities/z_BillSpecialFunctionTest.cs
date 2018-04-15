using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillSpecialFunctionTest : BaseWMSObjectTest<BillSpecialFunction>
    {
        protected override void FillRequiredFields(BillSpecialFunction obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().SPECIALFUNCTIONCODE = TestString;
            obj.AsDynamic().SPECIALFUNCTIONNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SPECIALFUNCTIONCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BillSpecialFunction obj)
        {
            obj.AsDynamic().SPECIALFUNCTIONDESC = TestString;
        }

        protected override void CheckSimpleChange(BillSpecialFunction source, BillSpecialFunction dest)
        {
            string sourceName = source.AsDynamic().SPECIALFUNCTIONDESC;
            string destName = dest.AsDynamic().SPECIALFUNCTIONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}