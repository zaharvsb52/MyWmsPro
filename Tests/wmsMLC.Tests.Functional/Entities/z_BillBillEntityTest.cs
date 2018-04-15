using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillBillEntityTest : BaseWMSObjectTest<BillBillEntity>
    {
        protected override void FillRequiredFields(BillBillEntity obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().BILLENTITYCODE = TestString;
            obj.AsDynamic().BILLENTITYEVENTFIELD = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BILLENTITYCODE = '{0}')", TestString);
        }
    }
}