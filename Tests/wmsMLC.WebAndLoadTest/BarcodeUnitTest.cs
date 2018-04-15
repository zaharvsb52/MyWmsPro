using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;

namespace wmsMLC.WebAndLoadTest
{
    [TestClass]
    public class BarcodeUnitTest : UnitTestWrapper
    {
        [TestMethod]
        public void InitBarcodeTest()
        {
            UnitTestHelper.Initialize(TestContext, typeof(BarcodeTest));
        }

        [TestMethod]
        public void TerminateBarcodeTest()
        {
            UnitTestHelper.Terminate(TestContext);
        }

        [TestMethod]
        public void BarcodeTest()
        {
            UnitTestHelper.Run(TestContext);
        }
    }

    public class BarcodeTest : UnitTestBase
    {
        private const string BarcodeCountParameterName = "BarcodeCount";
        private string[] _barcodes;
        private const int BarcodeCountDefault = 100;
        private int _barcodeCount;

        public override void Initialize(IDictionary parameters)
        {
            base.Initialize(parameters);
            _barcodeCount = parameters.Contains(BarcodeCountParameterName)
                ? (int)SerializationHelper.ConvertToTrueType(parameters[BarcodeCountParameterName], typeof(int)) : BarcodeCountDefault;
            // получаем ШК
            var filter = "barcode2entity = 'SKU' and rownum < " + _barcodeCount + 1;
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Barcode>>())
                _barcodes = mgr.GetFiltered(filter, null).Select(i => i.BarcodeValue).ToArray();
        }

        public override void Run()
        {
            //Log.DebugFormat("{0}: Start scan {1} BC in {2} sec with pause {3} sec", clientName, barcodeCount, scanTime.TotalSeconds, pauseTime.TotalSeconds);
            var rnd = new Random();
            var sw = new Stopwatch();
            sw.Start();
            using (var mgr = IoC.Instance.Resolve<IBaseManager<SKU>>())
            {
                for (int i = 0; i < _barcodeCount; i++)
                {
                    var itemStart = sw.Elapsed;
                    var barcode = _barcodes[rnd.Next(_barcodes.Length - 1)];
                    //var filter = string.Format("BARCODEL.BARCODEVALUE = '{0}'", barcode);
                    var filter = string.Format("skuid in (select barcodekey from wmsbarcode where barcode2entity = 'SKU' and barcodevalue='{0}')", barcode);
                    var attr = FilterHelper.GetAttrEntity<SKU>(SKU.SKUIDPropertyName, SKU.ArtCodePropertyName);
                    var skuList = mgr.GetFiltered(filter, attr).ToArray();
                    var itemTime = sw.Elapsed - itemStart;
                    //Log.DebugFormat("{0}: scan {1} by BC '{2}' - receive {3} sku per {4}", clientName, i, barcode, skuList.Length, itemTime);
                }
            }
            sw.Stop();
            //Log.DebugFormat("{0}: Stop scan. State '{1}' ({2}{3}{4})", clientName, sw.Elapsed < scanTime ? "OK" : "ERR", sw.Elapsed, sw.Elapsed < scanTime ? "<" : ">=", scanTime);
        }
    }
}
