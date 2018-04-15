using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class StatusTest : BaseWMSObjectTest<Status>
    {
        protected override void FillRequiredFields(Status obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().STATUSCODE = TestString;
            obj.AsDynamic().STATUSNAME = TestString;
            obj.AsDynamic().STATUS2ENTITY = "WAREHOUSE";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(STATUSCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Status obj)
        {
            obj.AsDynamic().STATUSDESC = TestString;
        }

        protected override void CheckSimpleChange(Status source, Status dest)
        {
            string sourceName = source.AsDynamic().STATUSDESC;
            string destName = dest.AsDynamic().STATUSDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}