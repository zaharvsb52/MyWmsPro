using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillEvent2BillerTest : BaseEntityTest<BillEvent2Biller>
    {
        public const decimal ExistsItem1Id = -1;

        protected override void FillRequiredFields(BillEvent2Biller entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLEVENT2BILLEREVENTHEADERID = EventHeaderTest.ExistsItem1Id;
            obj.BILLEVENT2BILLERBILLERCODE = BillBillerTest.ExistsItem2Code;
        }
    }
}