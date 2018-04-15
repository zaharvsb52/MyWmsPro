using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ExternalTrafficTest : BaseEntityTest<ExternalTraffic>
    {
        public const decimal ExistsItem1Code = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = ExternalTraffic.ExternalTrafficTrailerRNPropertyName;
        }
    }
}