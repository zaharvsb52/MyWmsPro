using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class CalendarTest : BaseEntityTest<Calendar>
    {
        public const decimal ExistsItem1Code = -1;
        public const decimal ExistsItem2Code = -2;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Calendar.CALENDARTIMETILLPropertyName;
        }

        protected override void FillRequiredFields(Calendar entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CALENDARDATE = DateTime.Now;
            obj.CALENDARTIMEFROM = DateTime.Now;
            obj.CALENDARTIMETILL = DateTime.Now;
        }
    }
}