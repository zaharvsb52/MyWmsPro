using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Calendar2MandantTest : BaseEntityTest<Calendar2Mandant>
    {
        protected override void FillRequiredFields(Calendar2Mandant entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CALENDAR2MANDANTCALENDARID = CalendarTest.ExistsItem2Code;
            obj.MANDANTID = TstMandantId;
            obj.CALENDAR2MANDANTOVERTIME = true;
        }
    }
}