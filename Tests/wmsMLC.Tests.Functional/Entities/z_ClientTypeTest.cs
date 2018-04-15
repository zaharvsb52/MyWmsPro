using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ClientTypeTest : BaseWMSObjectTest<ClientType>
    {
        protected override void FillRequiredFields(ClientType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().CLIENTTYPECODE = TestString;
            obj.AsDynamic().CLIENTTYPENAME = TestString;
            obj.AsDynamic().CLIENTTYPEDESC = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("CLIENTTYPECODE = '{0}'", TestString);
        }

        protected override void MakeSimpleChange(ClientType obj)
        {
            obj.AsDynamic().CLIENTTYPENAME = TestString + "002";
            obj.AsDynamic().CLIENTTYPEDESC = TestString + "fdfdf";
        }

        protected override void CheckSimpleChange(ClientType source, ClientType dest)
        {
            string sourceName = source.AsDynamic().CLIENTTYPECODE;
            string destName = dest.AsDynamic().CLIENTTYPECODE;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTTYPENAME;
            destName = dest.AsDynamic().CLIENTTYPENAME;
            destName.ShouldBeEquivalentTo(sourceName);

            sourceName = source.AsDynamic().CLIENTTYPEDESC;
            destName = dest.AsDynamic().CLIENTTYPEDESC;
            destName.ShouldBeEquivalentTo(sourceName);
        }
    }
}
