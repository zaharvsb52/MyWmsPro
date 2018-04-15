using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BLToolkit.Aspects;
using NUnit.Framework;

namespace wmsMLC.Business.Tests
{
    /*
    [TestFixture]
    public class DataFieldHelperTest
    {
        [SetUp]
        public virtual void Setup()
        {
            BLHelper.InitBL();
        }

        [Test]
        public void BaseCacheTest()
        {
            var fields1 = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.LookUp);
            fields1[0].Caption += "1111111111";
            var fields2 = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.LookUp);
            Assert.AreEqual(fields1[0].Caption, fields2[0].Caption);
        }

        [Test]
        public void ClearCacheTest()
        {
            var fields1 = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.LookUp);
            fields1[0].Caption += "1111111111";
            CacheAspect.ClearCache(typeof(DataFieldHelper));
            var fields2 = DataFieldHelper.Instance.GetDataFields(typeof(TE), SettingDisplay.LookUp);
            Assert.AreNotEqual(fields1[0].Caption, fields2[0].Caption);
        }

        [Test]
        public void ThreadTest()
        {
            BaseCacheTest();

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var task = new Task((state) =>
                {
                    Thread.CurrentThread.Name = state.ToString();
                    var startTic = DateTime.Now.Ticks;
                    Trace.WriteLine("start thread " + Thread.CurrentThread.Name);
                    try
                    {
                        BaseCacheTest();
                    }
                    catch (Exception)
                    {
                        Trace.WriteLine("exception in thread " + Thread.CurrentThread.Name);
                    }
                    Trace.WriteLine(string.Format("stop thread {0}: ticks={1}", Thread.CurrentThread.Name, DateTime.Now.Ticks - startTic));
                }, i);
                tasks.Add(task);
                task.Start();
            }

            Task.WaitAll(tasks.ToArray(), 30000);
        }
    }
    */
}