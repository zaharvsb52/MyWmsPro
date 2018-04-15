using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ClientTypeTest : BaseEntityTest<ClientType>
    {
        public const string ExistsItem1Code = "TST_CLIENTTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CLIENTTYPEDESC";
        }

        protected override void FillRequiredFields(ClientType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CLIENTTYPENAME = TestString;
        }
    }
}