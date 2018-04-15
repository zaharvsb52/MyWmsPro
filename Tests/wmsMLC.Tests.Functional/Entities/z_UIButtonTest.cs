using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class UIButtonTest : BaseWMSObjectTest<UIButton>
    {
        protected override void FillRequiredFields(UIButton obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().UIBUTTONCODE = TestString;
            obj.AsDynamic().UIBUTTONPANEL = TestString;
            obj.AsDynamic().UIBUTTONORDER = TestDecimal;
            obj.AsDynamic().UIBUTTONCAPTION = TestString;
            obj.AsDynamic().UIBUTTONHINT = TestString;
            obj.AsDynamic().UIBUTTONHOTKEY = TestString;
            obj.AsDynamic().UIBUTTONIMAGE = TestString;
            obj.AsDynamic().UIBUTTONENABLEFILTER = TestString;
            obj.AsDynamic().UIBUTTONVISIBLEFILTER = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(UIBUTTONCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(UIButton obj)
        {
            obj.AsDynamic().UIBUTTONDESC = TestString;
        }

        protected override void CheckSimpleChange(UIButton source, UIButton dest)
        {
            string sourceName = source.AsDynamic().UIBUTTONDESC;
            string destName = dest.AsDynamic().UIBUTTONDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}