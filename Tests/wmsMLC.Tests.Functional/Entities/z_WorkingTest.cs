using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WorkingTest : BaseWMSObjectTest<Working>
    {
        private readonly WorkTest _workTest = new WorkTest();
        private readonly WorkerGroupTest _workerGroupTest = new WorkerGroupTest();
        private readonly WorkerTest _workerTest = new WorkerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _workTest, _workerGroupTest, _workerTest };
        }

        protected override void FillRequiredFields(Working obj)
        {
            base.FillRequiredFields(obj);

            var work = _workTest.CreateNew();
            var group = _workerGroupTest.CreateNew();
            var worker = _workerTest.CreateNew();

            obj.AsDynamic().WORKINGID = TestDecimal;
            obj.AsDynamic().WORKID_R = work.GetKey();
            obj.AsDynamic().WORKERGROUPID_R = group.GetKey();
            obj.AsDynamic().WORKERID_R = worker.GetKey();
            obj.AsDynamic().WORKINGFROM = DateTime.Now;
            obj.AsDynamic().WORKINGMEASURE = TestString;
            obj.AsDynamic().WORKINGDOC = TestString;
            obj.AsDynamic().WORKINGERROR = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Working obj)
        {
            obj.AsDynamic().WORKINGTILL = DateTime.Now;
        }

        protected override void CheckSimpleChange(Working source, Working dest)
        {
            DateTime sourceName = source.AsDynamic().WORKINGTILL;
            DateTime destName = dest.AsDynamic().WORKINGTILL;
            (sourceName.ToString()).ShouldBeEquivalentTo(destName.ToString());
        }
    }
}