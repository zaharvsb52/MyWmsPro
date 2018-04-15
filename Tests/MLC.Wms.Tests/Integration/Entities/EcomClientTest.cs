using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EcomClientTest : BaseEntityTest<EcomClient>
    {
        //public const string ExistsItem1Code = "TST_ECOMCLIENT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = EcomClient.ClientlastnamePropertyName;
        }

        protected override void FillRequiredFields(EcomClient entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.ClientName = TestString;
            obj.ClientMiddleName = TestString;
            obj.ClientPhoneMobile = TestString;
            obj.ClientPhoneWork = TestString;
            obj.ClientPhoneInternal = TestString;
            obj.ClientPhoneHome = TestString;
            obj.ClientEmail = TestString;
            obj.ClientHostRef = TestString;
        }
    }
}