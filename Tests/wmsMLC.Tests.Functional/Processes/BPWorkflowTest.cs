using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects.Processes;

namespace wmsMLC.Tests.Functional.Processes
{
    [TestFixture, Ignore("Временно в игноре")]
    public class BPWorkflowTest : BaseWMSObjectTest<BPWorkflow>
    {
        private const string TestString = "AutoTest";

        protected override string GetCheckFilter()
        {
            return string.Format("({0} = '{1}')", "WorkflowCode", TestString);
        }

        protected override void FillRequiredFields(BPWorkflow obj)
        {
            // создаем обязательные объекты
            var bp = new BPProcessTest().CreateNew();

            // заполняем поля
            obj.AsDynamic().WorkflowCode = TestString;
            obj.AsDynamic().ProcessCode_r = bp.GetKey();
            obj.AsDynamic().WorkflowName = TestString;
        }

        public override void ClearForSelf(BPWorkflow obj)
        {
            base.ClearForSelf(obj);

            // очищаем зависимости
            new BPProcessTest().ClearForSelfByKey(obj.AsDynamic().ProcessCode_r);
        }

        protected override void MakeSimpleChange(BPWorkflow obj)
        {
            obj.AsDynamic().WorkflowDesc = TestString;
        }

        protected override void CheckSimpleChange(BPWorkflow source, BPWorkflow dest)
        {
            string sourceChange = source.AsDynamic().WorkflowDesc;
            string destChange = dest.AsDynamic().WorkflowDesc;
            // проверяем
            sourceChange.ShouldBeEquivalentTo(destChange);
        }
    }
}