using System;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ClientSessionTest : BaseEntityTest<ClientSession>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = ClientSession.ClientSessionUserDomainNamePropertyName;
        }

        protected override void FillRequiredFields(ClientSession entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CLIENTCODE_R = ClientTest.ExistsItem1Code;
            obj.CLIENTTYPECODE_R = ClientTypeTest.ExistsItem1Code;
            obj.CLIENTSESSIONBEGIN = DateTime.Now;
            obj.CLIENTSESSIONEND = DateTime.Now.AddHours(2);
            obj.CLIENTSESSIONAPPKEY = TestString;
            obj.CLIENTSESSIONCORRECTLYOFF = false;
            obj.USERCODE_R = CurrentUser;
        }
    }
}