using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class GlobalParamTest : BaseWMSObjectTest<GlobalParam>
    {
        protected override string GetCheckFilter()
        {
            return string.Format("(GLOBALPARAMCODE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(GlobalParam obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().GLOBALPARAMCODE = TestString;
        }

        protected override void MakeSimpleChange(GlobalParam obj)
        {
            obj.AsDynamic().GLOBALPARAMNAME = TestString;
        }

        protected override void CheckSimpleChange(GlobalParam source, GlobalParam dest)
        {
            string sourceName = source.AsDynamic().GLOBALPARAMNAME;
            string destName = dest.AsDynamic().GLOBALPARAMNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}
