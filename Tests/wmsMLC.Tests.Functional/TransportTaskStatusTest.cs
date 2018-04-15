using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional
{
    [TestFixture]
    public class TransportTaskStatusTest : BaseWMSObjectTest<TransportTaskStatus>
    {
        protected override void FillRequaredFields(TransportTaskStatus obj)
        {
            base.FillRequaredFields(obj);

            obj.AsDynamic().TTASKSTATUSCODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TTASKSTATUSCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(TransportTaskStatus obj)
        {
            obj.AsDynamic().TTASKSTATUSNAME = TestString;
        }

        protected override void CheckSimpleChange(TransportTaskStatus source, TransportTaskStatus dest)
        {
            string sourceName = source.AsDynamic().TTASKSTATUSNAME;
            string destName = dest.AsDynamic().TTASKSTATUSNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}