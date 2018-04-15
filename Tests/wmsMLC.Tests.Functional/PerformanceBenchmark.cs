using System;
using System.Diagnostics;
using System.Dynamic;
using BLToolkit.Data;
using NUnit.Framework;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;

namespace wmsMLC.Tests.Functional
{
    [TestFixture]
    public class PerformanceBenchmark : BaseWMSTest
    {
        [Test]
        public void ReadAll()
        {
            const int cnt = 1000;

            using (var mgr = IoC.Instance.Resolve<IBaseManager<CarType>>())
            {
                var items = mgr.GetFiltered("1 = 0");
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < cnt; i++)
            {
                using (var mgr = IoC.Instance.Resolve<IBaseManager<CarType>>())
                {
                    var items = mgr.GetFiltered("CARTYPEMODEL like '%уем%'");
                    Assert.IsNotEmpty(items);
                }
            }
            sw.Stop();

            Console.WriteLine("Total time in ms {0}. Avg time per call {1}", sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / cnt);
        }

        [Test]
        public void ReadData()
        {
            const int cnt = 1000;

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < cnt; i++)
            {
                using (var db = new DbManager())
                {
                    var items = db
                        .SetCommand("select * from wmscartype where CARTYPEMODEL like '%уем%' order by cartypeid desc")
                        .ExecuteList<ExpandoObject>();
                    //Assert.IsNotEmpty(items);
                }
            }
            sw.Stop();

            Console.WriteLine("Total time in ms {0}. Avg time per call {1}", sw.ElapsedMilliseconds, sw.ElapsedMilliseconds / cnt);
        }
    }
}