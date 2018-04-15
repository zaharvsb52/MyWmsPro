using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Managers.Processes;
using wmsMLC.Business.Objects;
using wmsMLC.Business.Objects.Processes;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional.Entities
{
    //[TestFixture]
    //public class Test001 : BaseWMSTest
    //{
    //    [Test]
    //    public void Test01()
    //    {
    //        using (var manager = IoC.Instance.Resolve<IBPProcessManager>())
    //        {
    //            var table = manager.GetInfoBySql(string.Format("MAX(P.TECODE_R) AS TECODE" +
    //                                                               ",MAX(TE.TECURRENTPLACE) AS TECURRENTPLACE, MAX(PL.PLACENAME) AS TECURRENTPLACE_NAME" +
    //                                                               ",SUM(P.PRODUCTCOUNTSKU) AS PRODUCTCOUNTSKU, SUM(DECODE(P.STATUSCODE_R, 'PRODUCT_BUSY', P.PRODUCTCOUNTSKU, 0)) AS RESCOUNTSKU" +
    //                                                               ",MAX(SKU.SKUNAME) AS SKUNAME" +
    //                                                               " FROM WMSPRODUCT P" +
    //                                                               " JOIN WMSSKU SKU ON P.SKUID_R = SKU.SKUID" +
    //                                                               " JOIN WMSTE TE ON P.TECODE_R = TE.TECODE" +
    //                                                               " JOIN WMSPLACE PL ON TE.TECURRENTPLACE = PL.PLACECODE" +
    //                                                               " WHERE SKU.ARTCODE_R = '{0}'" +
    //                                                               " GROUP BY TE.TECURRENTPLACE, P.TECODE_R, P.SKUID_R" +
    //                                                               " ORDER BY TECURRENTPLACE_NAME", "MO145544"));
    //            var t = 0;
    //            //table.Rows[0]["TECURRENTPLACE_NAME"]
    //        }
    //    }
    //}

    [TestFixture]
    public class ProductTest : BaseWMSObjectTest<Product>
    {
        private readonly IWBPosTest _IWBPosTest = new IWBPosTest();
        private readonly SKUTest _SKUTest = new SKUTest();
        private readonly TETest _TETest = new TETest();
        private readonly QlfTest _QlfTest = new QlfTest();
        private readonly MandantTest _mandantTest = new MandantTest();
        private const string PrDateMet = "DATETIME";
        private const string Qltype = "QLFNORMAL";

        //[Test]
        //public void TestGroup()
        //{
        //    var manager = CreateManager();
        //    var products = manager.GetFiltered("ARTCODE_R = 'ROM10347263'", GetModeEnum.Partial).ToArray();

        //    IGrouping<decimal, Product>[] results = products.GroupBy(p => p.SKUID).ToArray();

        //    foreach (var p in results)
        //    {
        //        var y = p.Sum(i => i.ProductCountSKU);
        //    }
        //}

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _IWBPosTest, _SKUTest, _TETest, _QlfTest, _mandantTest };
        }

        protected override void FillRequiredFields(Product obj)
        {
            base.FillRequiredFields(obj);

            _IWBPosTest.TestString = TestString + "1";
            _IWBPosTest.TestDecimal = TestDecimal + 1;
            var IWBPos = _IWBPosTest.CreateNew();

            _SKUTest.TestString = TestString + "2";
            _SKUTest.TestDecimal = TestDecimal + 2;
            var SKU = _SKUTest.CreateNew();

            _TETest.TestString = TestString + "3";
            _TETest.TestDecimal = TestDecimal + 3;
            var TE = _TETest.CreateNew();

            _mandantTest.TestString = TestString + "4";
            _mandantTest.TestDecimal = TestDecimal + 4;

            var QLF = _QlfTest.CreateNew();

            obj.AsDynamic().PRODUCTID = TestDecimal;
            obj.AsDynamic().IWBPOSID_R = IWBPos.GetKey();
            obj.AsDynamic().SKUID_R = SKU.GetKey();
            obj.AsDynamic().TECODE_R = TE.GetKey();
            obj.AsDynamic().PRODUCTCOUNTSKU = TestDouble;
            obj.AsDynamic().PRODUCTINPUTDATE = DateTime.Now;
            obj.AsDynamic().PRODUCTCOUNT = TestDouble;
            //Эти поля захардкожены, не будет так, не заработает Get()
            obj.AsDynamic().PRODUCTINPUTDATEMETHOD = PrDateMet;
            obj.AsDynamic().QLFCODE_R = QLF.GetKey();
            obj.AsDynamic().PRODUCTTTEQUANTITY = TestDecimal;
            obj.AsDynamic().PRODUCTOWNER = _mandantTest.CreateNew().GetKey();
        }

        protected override void MakeSimpleChange(Product obj)
        {
            obj.AsDynamic().PRODUCTBATCH = "MakeSimpleChange"+TestString;
        }

        protected override void CheckSimpleChange(Product source, Product dest)
        {
            string sourceName = source.AsDynamic().PRODUCTBATCH;
            string destName = dest.AsDynamic().PRODUCTBATCH;
            sourceName.ShouldBeEquivalentTo(destName);
        }
        
        protected override string GetCheckFilter()
        {
            return string.Format("(PRODUCTID = '{0}')", TestDecimal);
        }

        [Test, Ignore("Зацикливание теста")]
        public override void ManagerCRUDTest()
        {

        }
        [Test, Ignore("Зацикливание теста")]
        public override void ManagerGetFilteredTest()
        {

        }
        [Test, Ignore("Зацикливание теста")]
        public override void ManagerGetAllTest()
        {

        }
    }
}