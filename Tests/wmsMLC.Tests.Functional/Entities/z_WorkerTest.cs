using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WorkerTest : BaseWMSObjectTest<Worker>
    {
        protected override void FillRequiredFields(Worker obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().WORKERID = TestDecimal;
            obj.AsDynamic().WORKERLASTNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKERID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Worker obj)
        {
            obj.AsDynamic().WORKERNAME = TestString;
        }

        protected override void CheckSimpleChange(Worker source, Worker dest)
        {
            string sourceName = source.AsDynamic().WORKERNAME;
            string destName = dest.AsDynamic().WORKERNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test]
        public void DeleteAddress()
        {
            var parentMgr = CreateManager();
            var parent = CreateNew();
            var parentKey = parent.GetKey();

            // создаём инстанс вложенной сущности
            var address = new AddressBook();

            try
            {
                #region .  Create  .

                var oldLength = 0;

                // считываем коллекцию вложенных сущностей
                var addressLst = parent.AsDynamic().WORKERADDRESS as IList<AddressBook>;
                if (addressLst == null)
                {
                    addressLst = new WMSBusinessCollection<AddressBook>();
                    parent.SetProperty("WORKERADDRESS", addressLst);
                }
                else oldLength = addressLst.Count;

                // заполняем ссылки и обязательные поля вложенной сущности
                address.AsDynamic().ADDRESSBOOKINDEX = TestDecimal;
                address.AsDynamic().ADDRESSBOOKTYPECODE = "ADR_LEGAL";

                // добавляем связь с вложенной сущностью, сохраняем
                addressLst.Add(address);
                parentMgr.Update(parent);

                // читаем из БД по ключу
                var inserted = parentMgr.Get(parentKey);
                var addressLstNew = inserted.AsDynamic().WORKERADDRESS as IList<AddressBook>;

                // проверка создания
                addressLstNew.Should().NotBeNull("Должны были получить хотя бы 1 элемент");
                addressLstNew.Count.ShouldBeEquivalentTo(oldLength + 1, "Manager должен создавать вложенные сущности");

                #endregion

                #region .  Delete  .

                // удалем связь с вложенной сущностью, сохраняем
                addressLst = inserted.AsDynamic().WORKERADDRESS as IList<AddressBook>;
                address = addressLst[addressLst.Count - 1];
                addressLst.Remove(address);
                parentMgr.Update(inserted);

                // убеждаемся, что корректно удалили
                var deleted = parentMgr.Get(parentKey);
                addressLstNew = deleted.AsDynamic().WORKERADDRESS as IList<AddressBook>;
                if (addressLstNew == null) oldLength.Equals(0);
                else addressLstNew.Count.ShouldBeEquivalentTo(oldLength, "Manager должен удалять вложенные сущности");

                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + " УДАЛИТЕ СТРОКУ ИЗ ТАБЛИЦЫ WMSADDRESSBOOK с ADDRESSBOOKINDEX = " + TestDecimal.ToString());
            }
            finally
            {
                ClearForSelf();
            }
        }

    }
}