using FluentAssertions;
using MLC.SvcClient.Impl;
using Moq;
using NUnit.Framework;

namespace MLC.SvcClient.Tests
{
    [TestFixture]
    public class ServiceClientTest
    {
        private IServiceClient _client;

        [TestFixtureSetUp]
        public void SetUp()
        {
            var metadata = new Mock<IMetadata>();
            metadata.Setup(i => i.GetActionNames()).Returns(new[] {"TestAction"});
            metadata.Setup(i => i.GetMethodNames("TestAction")).Returns(new[] {"DoEcho"});

            var provider = new Mock<IProvider>();
            provider.Setup(i => i.GetMetadata()).Returns(metadata.Object);
            provider.Setup(i => i.Execute(It.Is<Transaction>(j => j.Action == "TestAction" &&
                                                                  j.Method == "DoEcho" &&
                                                                  j.Parameters[0].Name == "par")))
                .Returns((Transaction t) => t.Parameters[0].Value);
            provider.Setup(i => i.Execute<string>(It.Is<Transaction>(j => j.Action == "TestAction" &&
                                                                  j.Method == "DoEcho" &&
                                                                  j.Parameters[0].Name == "par")))
                .Returns((Transaction t) => (string)t.Parameters[0].Value);

            var manager = new Manager();
            manager.AddProvider(provider.Object);
            _client = new ServiceClient(manager);
        }

        [Test]
        public void DynamicTest()
        {
            var echoResult = (string)_client.AsDynamic().TestAction.DoEcho(par: "TestDynamic");
            echoResult.Should().Be("TestDynamic");
        }

        [Test]
        public void ExecTest()
        {
            var echoResult = _client.Exec("TestAction", "DoEcho", new[] { new Parameter("par", "TestExec") }, typeof(string));
            echoResult.Should().Be("TestExec");
        }

        [Test]
        public void ExecGenericTest()
        {
            var echoResult = _client.Exec<string>("TestAction", "DoEcho", new[] { new Parameter("par", "TestExec") });
            echoResult.Should().Be("TestExec");
        }

        [Test]
        public async void ExecAsyncTest()
        {
            var echoResult = await _client.ExecAsync("TestAction", "DoEcho", new[] { new Parameter("par", "TestExecAsync") }, typeof(string));
            echoResult.Should().Be("TestExecAsync");
        }

        [Test]
        public async void ExecAsyncGenericTest()
        {
            var echoResult = await _client.ExecAsync<string>("TestAction", "DoEcho", new [] {new Parameter("par", "TestExecAsync")});
            echoResult.Should().Be("TestExecAsync");
        }
    }
}
