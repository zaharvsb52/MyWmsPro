using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CalendarTest : BaseWMSObjectTest<Calendar>
    {
        protected override void FillRequiredFields(Calendar obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CALENDARID = TestDecimal;
            obj.AsDynamic().CALENDARDATE = DateTime.Now;
            obj.AsDynamic().CALENDARTIMEFROM = DateTime.Now;
            obj.AsDynamic().CALENDARTIMETILL = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CALENDARID = '{0}')", TestDecimal);
        }
    }
}