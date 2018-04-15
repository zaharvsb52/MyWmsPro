using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillBiller2MandantTest : BaseEntityTest<BillBiller2Mandant>
    {
        protected override void FillRequiredFields(BillBiller2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.BILLBILLER2MANDANTPARTNERID = TstMandantId;
            obj.BILLBILLER2MANDANTBILLERCODE = BillBillerTest.ExistsItem1Code;
        }
    }
}