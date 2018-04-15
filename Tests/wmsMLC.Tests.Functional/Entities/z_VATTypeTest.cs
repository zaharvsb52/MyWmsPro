using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class VATTypeTest : BaseWMSObjectTest<VATType>
    {
        protected override void FillRequiredFields(VATType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().VATTYPECODE = TestString;
            obj.AsDynamic().VATTYPENAME = TestString;
            obj.AsDynamic().VATTYPEINTERESTRATE = TestDouble;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(VATTYPECODE = '{0}')", TestString);
        }
    }
}