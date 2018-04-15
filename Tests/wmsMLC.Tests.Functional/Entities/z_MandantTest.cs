using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class MandantTest : BaseWMSObjectTest<Mandant>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("(PARTNERID = '{0}')", TestDecimal);
        }

        protected override void FillRequiredFields(Mandant obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().MANDANTCODE = TestString;
            obj.AsDynamic().MANDANTNAME = TestString;
            obj.AsDynamic().MANDANTFULLNAME = TestString;
            obj.AsDynamic().MANDANTID = TestDecimal;
        }

        protected override void MakeSimpleChange(Mandant obj)
        {
            obj.AsDynamic().MANDANTEMAIL = TestString;
        }

        protected override void CheckSimpleChange(Mandant source, Mandant dest)
        {
            string sourceName = source.AsDynamic().MANDANTEMAIL;
            string destName = dest.AsDynamic().MANDANTEMAIL;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override decimal TestDecimal
        {
            get
            {
                return base.TestDecimal;
            }
            set
            {
                //NOTE: не 123 потому что совпадет с partner - а это одна сущность
                base.TestDecimal = value + 1;
            }
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
                var addressLst = parent.AsDynamic().ADDRESS as IList<AddressBook>;
                if (addressLst == null)
                {
                    addressLst = new WMSBusinessCollection<AddressBook>();
                    parent.SetProperty("ADDRESS", addressLst);
                }
                else oldLength = addressLst.Count;

                // заполняем ссылки и обязательные поля вложенной сущности
                address.AsDynamic().ADDRESSBOOKINDEX = TestDecimal;
                address.AsDynamic().ADDRESSBOOKTYPECODE = "ADR_LEGAL";

                // добавляем связь с вложенной сущностью, сохраняем
                addressLst.Add(address);
                parentMgr.Update(parent);

                // читаем из БД по ключу
                parent = parentMgr.Get(parentKey);
                var addressLstNew = parent.AsDynamic().ADDRESS as IList<AddressBook>;

                // проверка создания
                addressLstNew.Should().NotBeNull("Должны были получить хотя бы 1 элемент");
                addressLstNew.Count.ShouldBeEquivalentTo(oldLength + 1, "Manager должен создавать вложенные сущности");

                #endregion

                #region .  Delete  .

                // удалем связь с вложенной сущностью, сохраняем
                addressLst = parent.AsDynamic().ADDRESS as IList<AddressBook>;
                address = addressLst[addressLst.Count - 1];
                addressLst.Remove(address);

                parentMgr.Update(parent);

                // убеждаемся, что корректно удалили
                parent = parentMgr.Get(parentKey);
                addressLstNew = parent.AsDynamic().ADDRESS as IList<AddressBook>;
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