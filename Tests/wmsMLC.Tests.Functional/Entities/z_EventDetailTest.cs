using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture,Ignore("Detail получаем и пишем через Header, тестируем тамже")]
    public class EventDetailTest : BaseWMSObjectTest<EventDetail>
    {
        private readonly EventHeaderTest _eventHeaderTest = new EventHeaderTest();

        public EventDetailTest()
        {
            _eventHeaderTest.TestString = TestString;
        }

        protected override void FillRequiredFields(EventDetail obj)
        {
            base.FillRequiredFields(obj);

            var eventHeader = _eventHeaderTest.CreateNew();

            obj.AsDynamic().EVENTDETAILID = TestDecimal;
            obj.AsDynamic().EVENTHEADERID_R = eventHeader.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENTDETAILID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(EventDetail obj)
        {
            obj.AsDynamic().PRODUCTLOT = TestString;
        }

        protected override void CheckSimpleChange(EventDetail source, EventDetail dest)
        {
            string sourceName = source.AsDynamic().PRODUCTLOT;
            string destName = dest.AsDynamic().PRODUCTLOT;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _eventHeaderTest };
        }
    }
}