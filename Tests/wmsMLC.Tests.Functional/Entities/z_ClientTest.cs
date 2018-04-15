using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ClientTest : BaseWMSObjectTest<Client>
    {

        [Test]
        protected override void FillRequiredFields(Client obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CLIENTCODE = TestString;
            obj.AsDynamic().CLIENTNAME = TestString;
            obj.AsDynamic().CLIENTDESC = TestString;
            obj.AsDynamic().CLIENTMAC = TestString;
            obj.AsDynamic().CLIENTIP = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(CLIENTCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Client obj)
        {
            obj.AsDynamic().CLIENTNAME = "CLIENTNAME02";
            obj.AsDynamic().CLIENTDESC = "CLIENTDESC02";
            obj.AsDynamic().CLIENTMAC = "0987654321";
            obj.AsDynamic().CLIENTIP = "99988877";
        }

        protected override void CheckSimpleChange(Client source, Client dest)
        {
            string sourceName;
            string destName;

            sourceName = source.AsDynamic().CLIENTCODE;
            destName = dest.AsDynamic().CLIENTCODE;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTNAME;
            destName = dest.AsDynamic().CLIENTNAME;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTDESC;
            destName = dest.AsDynamic().CLIENTDESC;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTMAC;
            destName = dest.AsDynamic().CLIENTMAC;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTIP;
            destName = dest.AsDynamic().CLIENTIP;
            destName.ShouldBeEquivalentTo(sourceName);

        }

    }
}
