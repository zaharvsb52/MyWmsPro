using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture, Ignore("UNIQUE OBJECT! Can't CRUD like others do...")]
    public class EventHeaderTest : BaseWMSObjectTest<EventHeader>
    {
        private readonly PartnerTest _partnerTest = new PartnerTest();
        private readonly BillOperationTest _billOperationTest = new BillOperationTest();
        private readonly EventKindTest _eventKindTest = new EventKindTest();

        public EventHeaderTest()
        {
            _partnerTest.TestString = TestString;
            _billOperationTest.TestString = TestString;
            _eventKindTest.TestString = TestString;
        }

        protected override void FillRequiredFields(EventHeader obj)
        {
            base.FillRequiredFields(obj);

            var partner = _partnerTest.CreateNew();
            var billOperation = _billOperationTest.CreateNew();
            var eventKind = _eventKindTest.CreateNew();

            obj.AsDynamic().EVENTHEADERID = TestDecimal;
            obj.AsDynamic().PARTNERID_R = partner.GetKey();
            obj.AsDynamic().OPERATIONCODE_R = billOperation.GetKey();
            obj.AsDynamic().EVENTKINDCODE_R = eventKind.GetKey();
            obj.AsDynamic().PROCESSCODE_R = "TEGetInfo";
            obj.AsDynamic().EVENTHEADERINSTANCE = TestString;
            obj.AsDynamic().EVENTHEADERSTARTTIME = DateTime.Now;
        }

        [Test]
        public void ManagerCRUDTest2()
        {
            var mgr = CreateManager() as EventHeaderManager;

            // создаем новый экземпляр
            var obj = mgr.New();
            obj.Should().NotBeNull("Manager должен уметь создавать новый экземпляр объекта");

            // создаем все нужные для создания объекты и
            // заполняем обязательные поля
            FillRequiredFields(obj);

            // сохраняем в БД
            mgr.RegEvent(ref obj, eventDetail: null);

            // убеждаемся, что ключ заполнился
            var key = (decimal)((IKeyHandler)obj).GetKey();

            // читаем из БД по ключу
            obj = mgr.Get(key);
            obj.Should().NotBeNull("Мы должны уметь получать объект по ключу");

            // сравниваем ключи
            var insKey = ((IKeyHandler)obj).GetKey();
            insKey.Should()
                .NotBeNull("Ключ полученного объект должен соответствовать ключу переданного")
                .And.Be(key, "Ключи должны совпадать");

            // Обновляем
            MakeSimpleChange(obj);
            mgr.Update(obj);

            // читаем из БД по ключу
            var updated = mgr.Get(key);
            updated.Should().NotBeNull("Мы должны уметь получать объект по ключу");

            // проверяем, что изменения прошли
            CheckSimpleChange(obj, updated);

            // удаляем из БД
            ClearForSelf();

            // убеждаемся, что корректно удалили
            var deleted = mgr.Get(key);
            deleted.Should().BeNull("Удаленный объект нельзя получить из БД");
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(EVENTHEADERID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(EventHeader obj)
        {
            obj.AsDynamic().EVENTHEADERINSTANCE = TestString;
        }

        protected override void CheckSimpleChange(EventHeader source, EventHeader dest)
        {
            string sourceName = source.AsDynamic().EVENTHEADERINSTANCE;
            string destName = dest.AsDynamic().EVENTHEADERINSTANCE;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _partnerTest, _billOperationTest, _eventKindTest };
        }
    }
}