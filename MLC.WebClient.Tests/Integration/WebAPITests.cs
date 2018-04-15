using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MLC.SvcClient;
using MLC.SvcClient.Impl;
using MLC.SvcClient.Impl.ExtDirect;
using MLC.WebClient.Tests.Unit;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MLC.WebClient.Tests.Integration
{
    [TestFixture]
    public class WebAPITests
    {
        private IManager _manager;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var httpClientStore = new HttpClientStore();
            // auth
            AuthServiceTests.Authenticate(TestSettings.BaseUrl, httpClientStore);
            // init
            var provider = new ExtDirectProvider(TestSettings.BaseUrl, "rpc", httpClientStore, new JsonSerializer());
            _manager = new Manager();
            _manager.AddProvider(provider);
        }

        [Test]
        public void EchoTest()
        {
            DoEcho(new WmsAPI(_manager));
        }

        [Test, Ignore]
        public void EchoPerformanceTest()
        {
            var api = new WmsAPI(_manager);
            CalcPerformance(() => { DoEcho(api); });
        }

        [Test, Ignore]
        public void EchoPerformanceMultiThreadsTest()
        {
            var api = new WmsAPI(_manager);
            var taskList = new List<Task<long>>();
            var totalSw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                var task = Task<long>.Factory.StartNew(() => CalcPerformance(() => { DoEcho(api); }));
                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());
            totalSw.Stop();

            long totalCount = taskList.Sum(i => i.Result);
            Console.WriteLine("Total: Time={0};Calls={1};Speed={2} call/s", totalSw.Elapsed, totalCount, totalCount / totalSw.Elapsed.TotalSeconds);
        }

        [Test]
        public void GetAvailablePickListCountTest()
        {
            var api = new WmsAPI(_manager);
            var res = api.GetAvailablePickListCountAsync("FORK194").Result;
        }

        [Test]
        public void GetAvailableTransportTaskCountTest()
        {
            var api = new WmsAPI(_manager);
            var res = api.GetAvailableTransportTaskCountAsync(65792).Result;
        }

        [Test, Ignore]
        public void GetAvailablePickListCountPerformanceTest()
        {
            CalcPerformance(GetAvailablePickListCountTest);
        }

        private void DoEcho(WmsAPI api = null)
        {
            var res = api.Echo("test message");
            res.ShouldBeEquivalentTo("test message");
        }

        private long CalcPerformance(Action action)
        {
            long count = 0;
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 10000)
            {
                action();
                count++;
            }
            sw.Stop();
            Console.WriteLine("Thread={0},Time={1};Calls={2};Speed={3} call/s", Thread.CurrentThread.Name,
                sw.Elapsed, count, count/sw.Elapsed.TotalSeconds);
            return count;
        }
    }
}