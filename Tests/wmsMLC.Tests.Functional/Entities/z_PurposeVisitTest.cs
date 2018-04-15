using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PurposeVisitTest : BaseWMSObjectTest<PurposeVisit>
    {
        protected override void FillRequiredFields(PurposeVisit obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PURPOSEVISITID = TestDecimal;
            obj.AsDynamic().PURPOSEVISITCODE = TestString;
            obj.AsDynamic().PURPOSEVISITNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PURPOSEVISITID = '{0}')", TestDecimal);
        }
    }
}