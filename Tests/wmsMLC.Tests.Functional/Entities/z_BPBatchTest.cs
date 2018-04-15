using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BPBatchTest : BaseWMSObjectTest<BPBatch>
    {
        protected override void FillRequiredFields(BPBatch obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().BATCHCODE = TestString;
            obj.AsDynamic().BATCHNAME = TestString;
            // Пока BPWORKFLOW ignore - используем существующий БП
            //obj.AsDynamic().WORKFLOWCODE_R = "WFCLOSEPACK";
            obj.AsDynamic().WORKFLOWCODE_R = "WfFunctionTest";
            obj.AsDynamic().BATCHSQL = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BATCHCODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(BPBatch obj)
        {
            obj.AsDynamic().BATCHDESC = TestString;
        }

        protected override void CheckSimpleChange(BPBatch source, BPBatch dest)
        {
            string sourceName = source.AsDynamic().BATCHDESC;
            string destName = dest.AsDynamic().BATCHDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}