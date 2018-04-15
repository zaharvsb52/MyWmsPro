using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.DAL;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EventHeaderTest : BaseEntityTest<EventHeader>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = EventHeader.InstancePropertyName;
        }

        protected override void FillRequiredFields(EventHeader entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.OPERATIONCODE_R = BillOperationTest.ExistsItem1Code;
            obj.EVENTKINDCODE_R = EventKindTest.ExistsItem2Code;
            obj.EVENTHEADERINSTANCE = TestString;
            obj.EVENTHEADERSTARTTIME = DateTime.Now;
        }

        protected void FillRequiredDetailFields(EventDetail detail)
        {
            //base.FillRequiredFields(entity); //не требуется, ID генерится в БД безусловно
            dynamic obj = detail.AsDynamic();

            obj.COMMACTID_R = CommActTest.ExistsItem1Id;
        }

        [Test]
        public override void Entity_should_be_create_read_update_delete()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();

            using (var uow = uowFactory.Create())
            using (var mgr = IoC.Instance.Resolve<EventHeaderManager>())
            {
                mgr.SetUnitOfWork(uow);
                uow.BeginChanges();

                try
                {
                    // создаем новый экземпляр
                    var obj = mgr.New();
                    obj.Should().NotBeNull("Manager должен уметь создавать новый экземпляр объекта");
                    var detail = new EventDetail();

                    // создаем все нужные для создания объекты и
                    // заполняем обязательные поля
                    FillRequiredFields(obj);
                    FillRequiredDetailFields(detail);

                    // сохраняем в БД
                    mgr.RegEvent(ref obj, detail);

                    // убеждаемся, что ключ заполнился
                    var key = ((IKeyHandler)obj).GetKey();
                    key.Should().NotBeNull("У сохраненного объекта Ключ должен быть заполнен");

                    // Обновляем
                    if (MakeSimpleChange(obj))
                    {
                        mgr.Update(obj);

                        // читаем из БД по ключу
                        var updated = mgr.Get((decimal)key);
                        updated.Should().NotBeNull("Мы должны уметь получать объект по ключу");

                        // сравниваем ключи
                        var insKey = ((IKeyHandler)updated).GetKey();
                        insKey.Should()
                            .NotBeNull("Ключ полученного объект должен соответствовать ключу переданного")
                            .And.Be(key, "Ключи должны совпадать");

                        // проверяем, что изменения прошли
                        CheckSimpleChange(obj, updated);

                        // todo: other simple change (for locking check)
                    }

                    // события не удаляются, поэтому тест на удаление не нужен
                    // удаляем
                    //mgr.Delete(obj);
                    // сущность может быть кэшируемой
                    //mgr.ClearCache();
                    // убеждаемся, что корректно удалили
                    //var deleted = mgr.Get((decimal)key);
                    //deleted.Should().BeNull("Удаленный объект нельзя получить из БД");
                }
                finally
                {
                    // откатываем какие-либо изменения (чтобы не мусорить)
                    uow.RollbackChanges();
                }
            }
        }
    }
}