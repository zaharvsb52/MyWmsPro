using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration
{
    [TestFixture]
    public class GlobalParamTest
    {
        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL();
        }

        [Test]
        public void ManagerGetByEntityTest()
        {
            var mgr = new GlobalParamManager();
            var result = mgr.GetByEntity(typeof(TEType));
            result.Should().NotBeEmpty();
        }
    }
}
