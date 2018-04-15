using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WorkerGroupTest : BaseWMSObjectTest<WorkerGroup>
    {
        protected override void FillRequiredFields(WorkerGroup obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().WORKERGROUPID = TestDecimal;
            obj.AsDynamic().WORKERGROUPNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKERGROUPID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(WorkerGroup obj)
        {
            obj.AsDynamic().WORKERGROUPDESC = TestString;
        }

        protected override void CheckSimpleChange(WorkerGroup source, WorkerGroup dest)
        {
            string sourceName = source.AsDynamic().WORKERGROUPDESC;
            string destName = dest.AsDynamic().WORKERGROUPDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}