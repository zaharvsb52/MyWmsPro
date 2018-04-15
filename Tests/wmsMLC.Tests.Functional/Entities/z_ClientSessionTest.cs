using System;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ClientSessionTest : BaseWMSObjectTest<ClientSession>
    {
        private readonly ClientTest _clientTest = new ClientTest();
        private readonly ClientTypeTest _clientTypeTest = new ClientTypeTest();

        [Test]
        protected override void FillRequiredFields(ClientSession obj)
        {
            base.FillRequiredFields(obj);

            var client = _clientTest.CreateNew();
            var clienttype = _clientTypeTest.CreateNew();

            obj.AsDynamic().CLIENTCODE_R = client.GetKey();
            obj.AsDynamic().CLIENTTYPECODE_R = clienttype.GetKey();
            obj.AsDynamic().CLIENTSESSIONBEGIN = DateTime.Now;
            obj.AsDynamic().CLIENTSESSIONEND = DateTime.Now.AddHours(2);
            obj.AsDynamic().CLIENTSESSIONAPPKEY = TestString;
            obj.AsDynamic().CLIENTSESSIONCORRECTLYOFF = false;
            obj.AsDynamic().USERCODE_R = "TECH_AUTOTEST";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(upper(CLIENTCODE_R) like upper('{0}%'))", AutoTestMagicWord);
        }

        protected override void MakeSimpleChange(ClientSession obj)
        {
            obj.AsDynamic().CLIENTSESSIONBEGIN = DateTime.Now;
            obj.AsDynamic().CLIENTSESSIONEND = DateTime.Now.AddHours(2);
            obj.AsDynamic().CLIENTSESSIONAPPKEY = "22345";
            obj.AsDynamic().CLIENTSESSIONCORRECTLYOFF = true;
            obj.AsDynamic().USERCODE_R = "DEBUG";
        }

        protected override void CheckSimpleChange(ClientSession source, ClientSession dest)
        {
            object sourceName;
            object destName;

            sourceName = source.AsDynamic().CLIENTSESSIONAPPKEY;
            destName = dest.AsDynamic().CLIENTSESSIONAPPKEY;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTSESSIONCORRECTLYOFF;
            destName = dest.AsDynamic().CLIENTSESSIONCORRECTLYOFF;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().USERCODE_R;
            destName = dest.AsDynamic().USERCODE_R;
            destName.ShouldBeEquivalentTo(sourceName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _clientTest.ClearForSelf();
            _clientTypeTest.ClearForSelf();
        }
    }
}
