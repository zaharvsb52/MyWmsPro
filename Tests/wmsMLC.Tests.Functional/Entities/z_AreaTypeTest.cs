using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class AreaTypeTest : BaseWMSObjectTest<AreaType>
    {
        protected override void FillRequiredFields(AreaType obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().AREATYPECODE = TestString;
            obj.AsDynamic().AREATYPENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(AREATYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(AreaType obj)
        {
            obj.AsDynamic().AREATYPEDESC = TestString;
        }

        protected override void CheckSimpleChange(AreaType source, AreaType dest)
        {
            string sourceName = source.AsDynamic().AREATYPEDESC;
            string destName = dest.AsDynamic().AREATYPEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}