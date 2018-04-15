using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;

namespace wmsMLC.Tests.Functional.Processes
{
    [TestFixture, Ignore("Временно в игноре")]
    public class BPProcessTest : BaseWMSObjectTest<BPProcess>
    {
        private const string TestString = "AutoTest";

        protected override string GetCheckFilter()
        {
            return "((ProcessCode = '" + TestString + "' ))";
        }

        protected override void FillRequiredFields(BPProcess obj)
        {
            obj.AsDynamic().ProcessCode = TestString;
            obj.AsDynamic().ProcessName = TestString;
            obj.AsDynamic().ProcessLocked = false;
            obj.AsDynamic().ProcessExecutor = "Service";
            obj.AsDynamic().ProcessEngine = "Workflow";
        }

        protected override void MakeSimpleChange(BPProcess obj)
        {
            obj.AsDynamic().ProcessDesc = TestString;
        }

        protected override void CheckSimpleChange(BPProcess source, BPProcess dest)
        {
            string sourceChange = source.AsDynamic().ProcessDesc;
            string destChange = dest.AsDynamic().ProcessDesc;
            sourceChange.ShouldBeEquivalentTo(destChange);
        }
    }
}