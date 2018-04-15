using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SKUTest : BaseWMSObjectTest<SKU>
    {
        private readonly ArtTest _artTest = new ArtTest();
        private readonly MeasureTest _measureTest = new MeasureTest();
        private readonly TETypeTest _tteTest1 = new TETypeTest();
        private readonly TETypeTest _tteTest2 = new TETypeTest();
        
        public SKUTest()
        {
            _tteTest1.TestDecimal = TestDecimal;
            _tteTest1.TestString = TestString + "1";
            _tteTest2.TestDecimal = TestDecimal + 1;
            _tteTest2.TestString = TestString + "2";
            _artTest.TestDecimal = TestDecimal;
            _measureTest.TestDecimal = TestDecimal;
            _measureTest.TestString = TestString;
        }

        protected override void FillRequiredFields(SKU obj)
        {
            base.FillRequiredFields(obj);
            var art = _artTest.CreateNew();
            _measureTest.TestString = TestString;
            var m = _measureTest.CreateNew();

            obj.AsDynamic().SKUID = TestDecimal;
            obj.AsDynamic().ARTCODE_R = art.GetKey();
            obj.AsDynamic().MEASURECODE_R = m.GetKey();
            obj.AsDynamic().SKUCOUNT = TestDecimal;
            obj.AsDynamic().SKUNAME = TestString;
            obj.AsDynamic().SKUPRIMARY = 1;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SKUID = '{0}')", TestDecimal);

            //return string.Format("BARCODEL.BARCODEVALUE='{0}'", "123");
            //return
            //    string.Format(
            //        "SKUID in (select TO_NUMBER(b.BARCODEKEY) from WMSBARCODE b where b.BARCODE2ENTITY = 'SKU' AND b.BARCODEVALUE = '{0}')",
            //        "123");
        }

        protected override void MakeSimpleChange(SKU obj)
        {
            obj.AsDynamic().SKUDESC = TestString;
        }

        protected override void CheckSimpleChange(SKU source, SKU dest)
        {
            string sourceName = source.AsDynamic().SKUDESC;
            string destName = dest.AsDynamic().SKUDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _artTest, _measureTest, _tteTest1, _tteTest2 };
        }

        [Test,Ignore]
        public void SKU2TETypeTest()
        {
            // создаём инстанс связанной сущности
            var tte1 = _tteTest1.CreateNew();
            var tte2 = _tteTest2.CreateNew();

            // создаём инстанс вложенной сущности
            var sku2tte = new SKU2TTE();

            try
            {
                #region .  Create  .

                // создаём инстанс сущности
                var mgr = CreateManager();
                var obj = mgr.New();
                FillRequiredFields(obj);
                mgr.Insert(ref obj);
                var key = obj.GetKey();
                var oldLength = 0;

                // считываем коллекцию вложенных сущностей
                var sku2tteLst = obj.AsDynamic().TETYPE2SKU as IList<SKU2TTE>;
                if (sku2tteLst == null)
                {
                    sku2tteLst = new WMSBusinessCollection<SKU2TTE>();
                    obj.SetProperty("TETYPE2SKU", sku2tteLst);
                }
                else oldLength = sku2tteLst.Count;

                // заполняем ссылки и обязательные поля вложенной сущности
                sku2tte.AsDynamic().SKU2TTEID = TestDecimal;
                sku2tte.AsDynamic().SKU2TTETETYPECODE = tte1.GetKey();
                sku2tte.AsDynamic().SKU2TTESKUID = key;
                sku2tte.AsDynamic().SKU2TTEQUANTITY = TestDecimal;
                sku2tte.AsDynamic().SKU2TTEMAXWEIGHT = TestDecimal;
                sku2tte.AsDynamic().SKU2TTELENGTH = TestDecimal;
                sku2tte.AsDynamic().SKU2TTEWIDTH = TestDecimal;
                sku2tte.AsDynamic().SKU2TTEHEIGHT = TestDecimal;

                //new
                 CreateManager<SKU2TTE>().Insert(ref sku2tte);

                // добавляем связь с вложенной сущностью, сохраняем
                sku2tteLst.Add(sku2tte);
                mgr.Update(obj);


                // читаем из БД по ключу
                obj = mgr.Get(key);
                var sku2tteLstNew = obj.AsDynamic().TETYPE2SKU as IList<SKU2TTE>;

                // проверка создания
                sku2tteLstNew.Should().NotBeNull("Должны были получить хотя бы 1 элемент");
                var newLength = sku2tteLstNew.Count;
                newLength.ShouldBeEquivalentTo(oldLength + 1, "Manager должен создавать вложенные сущности");

                #endregion

                #region .  Update simple  .

                // сохраняем старое значение не ключевого поля
                sku2tte = sku2tteLstNew[sku2tteLstNew.Count - 1];
                var oldValue = sku2tte.AsDynamic().SKU2TTEHEIGHT;

                // меняем значение
                sku2tte.AsDynamic().SKU2TTEHEIGHT = TestDecimal + 1;
                sku2tteLstNew[sku2tteLstNew.Count - 1] = sku2tte;

                // сохраняем в базе
                mgr.Update(obj);

                // вычитываем из базы
                obj = mgr.Get(key);
                sku2tteLstNew = obj.AsDynamic().TETYPE2SKU as IList<SKU2TTE>;
                sku2tte = sku2tteLstNew[sku2tteLstNew.Count - 1];

                // проверяем изменение
                var newValue = sku2tte.AsDynamic().SKU2TTEHEIGHT;
                newValue.Equals(oldValue + 1);

                #endregion

                #region .  Update link  .

                // меняем значение ссылочного поля
                sku2tte.AsDynamic().SKU2TTETETYPECODE = tte2.GetKey();
                sku2tteLstNew[sku2tteLstNew.Count - 1] = sku2tte;

                // сохраняем в базе
                mgr.Update(obj);

                // вычитываем из базы
                obj = mgr.Get(key);
                sku2tteLstNew = obj.AsDynamic().TETYPE2SKU as IList<SKU2TTE>;
                sku2tte = sku2tteLstNew[sku2tteLstNew.Count - 1];

                // проверяем изменение
                var newLink = sku2tte.AsDynamic().SKU2TTETETYPECODE;
                newLink.Equals(tte2.GetKey());

                #endregion

                #region .  Delete  .

                // удалем связь с вложенной сущностью, сохраняем
                sku2tteLst = obj.AsDynamic().TETYPE2SKU as IList<SKU2TTE>;
                sku2tte = sku2tteLst[sku2tteLst.Count - 1];
                sku2tteLst.Remove(sku2tte);
                mgr.Update(obj);

                //new 
                CreateManager<SKU2TTE>().Delete(sku2tte);

                // убеждаемся, что корректно удалили
                var deleted = mgr.Get(key);
                sku2tteLstNew = deleted.AsDynamic().TETYPE2SKU as IList<SKU2TTE>;
                if (sku2tteLstNew == null) oldLength.Equals(0);
                else sku2tteLstNew.Count.ShouldBeEquivalentTo(oldLength, "Manager должен удалять вложенные сущности");

                #endregion

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                #region .  ClearForSelf  .

                // чистим за собой
                if (sku2tte != null)
                {
                    var sku2tteMgr = CreateManager<SKU2TTE>();
                    sku2tteMgr.Delete(sku2tte);
                }
                ClearForSelf();

                #endregion
            }
        }
    }
}