using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Calendar2MandantTest : BaseWMSObjectTest<Calendar2Mandant>
    {
        private readonly MandantTest _mandantTest = new MandantTest();
        private readonly CalendarTest _calendarTest = new CalendarTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest, _calendarTest };
        }

        protected override void FillRequiredFields(Calendar2Mandant obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CALENDAR2MANDANTID = TestDecimal;
            obj.AsDynamic().CALENDAR2MANDANTCALENDARID = _calendarTest.CreateNew().GetKey();
            obj.AsDynamic().MANDANTID = _mandantTest.CreateNew().GetKey();
            obj.AsDynamic().CALENDAR2MANDANTOVERTIME = true;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALENDAR2MANDANTID = '{0}')", TestDecimal);
        }
    }
}