using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PartnerTest : BaseWMSObjectTest<Partner>
    {
        public const int CountRow = 500;
        //private readonly MandantTest _mandantTest = new MandantTest();

        [Test(Description = "Получение партнеров по фильтру с ограничением по времени")]
        [MaxTime(5000)]
        public void GetFilteredPartnersTest()
        {
            var mgr = CreateManager();
            var filter = string.Format("((ROWNUM < '{0}' ))", CountRow);
            mgr.GetFiltered(filter, GetModeEnum.Partial);
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PARTNERID = '{0}')", TestDecimal);
        }

        protected override void FillRequiredFields(Partner obj)
        {
            base.FillRequiredFields(obj);
            
            //var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().PARTNERID = TestDecimal;
            obj.AsDynamic().PARTNERCODE = TestDecimal.ToString();
            obj.AsDynamic().MANDANTID = 1;// mandant.GetKey();
            obj.AsDynamic().PARTNERNAME = TestDecimal.ToString();
        }

        protected override void MakeSimpleChange(Partner obj)
        {
            obj.AsDynamic().PartnerName = TestString;
        }

        protected override void CheckSimpleChange(Partner source, Partner dest)
        {
            string sourceName = source.AsDynamic().PartnerName;
            string destName = dest.AsDynamic().PartnerName;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        //protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        //{
        //    return new BaseWMSTest[] { _mandantTest };
        //}

        [Test,Ignore("Хистори нет")]
        public override void ManagerGetHistoryTest()
        {
            
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
                var inserted = parentMgr.Get(parentKey);
                var addressLstNew = inserted.AsDynamic().ADDRESS as IList<AddressBook>;

                // проверка создания
                addressLstNew.Should().NotBeNull("Должны были получить хотя бы 1 элемент");
                addressLstNew.Count.ShouldBeEquivalentTo(oldLength + 1, "Manager должен создавать вложенные сущности");

                #endregion

                #region .  Delete  .

                // удалем связь с вложенной сущностью, сохраняем
                addressLst = inserted.AsDynamic().ADDRESS as IList<AddressBook>;
                address = addressLst[addressLst.Count - 1];
                addressLst.Remove(address);
                parentMgr.Update(inserted);
                // убеждаемся, что корректно удалили
                var deleted = parentMgr.Get(parentKey);
                addressLstNew = deleted.AsDynamic().ADDRESS as IList<AddressBook>;
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
