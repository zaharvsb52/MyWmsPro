using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class TransportTaskTypeTest : BaseWMSObjectTest<TransportTaskType>
    {
        protected override void FillRequiredFields(TransportTaskType obj)
        {
            base.FillRequiredFields(obj);
            obj.AsDynamic().TTASKTYPECODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TTASKTYPECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(TransportTaskType obj)
        {
            obj.AsDynamic().TTASKTYPENAME = TestString;
        }

        protected override void CheckSimpleChange(TransportTaskType source, TransportTaskType dest)
        {
            string sourceName = source.AsDynamic().TTASKTYPENAME;
            string destName = dest.AsDynamic().TTASKTYPENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет хистори")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}