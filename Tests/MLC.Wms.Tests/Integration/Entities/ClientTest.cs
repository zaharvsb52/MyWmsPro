using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ClientTest : BaseEntityTest<Client>
    {
        public const string ExistsItem1Code = "TST_CLIENT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CLIENTDESC";
        }

        protected override void FillRequiredFields(Client entity)
        {
            base.FillRequiredFields(entity);

            entity.ClientName = TestString;
        }
    }
}