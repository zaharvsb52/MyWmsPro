using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class ReceiveAreaTest : BaseWMSObjectTest<ReceiveArea>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("(RECEIVEAREACODE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(ReceiveArea obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().RECEIVEAREACODE = TestString;
            obj.AsDynamic().RECEIVEAREANAME = TestString;
        }

        protected override void MakeSimpleChange(ReceiveArea obj)
        {
            obj.AsDynamic().RECEIVEAREADESC = TestString;
        }

        protected override void CheckSimpleChange(ReceiveArea source, ReceiveArea dest)
        {
            string sourceName = source.AsDynamic().RECEIVEAREADESC;
            string destName = dest.AsDynamic().RECEIVEAREADESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}