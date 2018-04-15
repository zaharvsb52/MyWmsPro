using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class EpsTask2JobTest : BaseWMSObjectTest<EpsTask2Job>
    {
        public const bool TestBool = false;
        
        private readonly EpsJobTest _epsJobTest = new EpsJobTest();
        private readonly EpsTaskTest _epsTaskTest = new EpsTaskTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _epsJobTest, _epsTaskTest };
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TASK2JOBID = '{0}')", TestDecimal);
        }

        protected override void FillRequiredFields(EpsTask2Job obj)
        {
            var job = _epsJobTest.CreateNew();
            var task = _epsTaskTest.CreateNew();
           
            base.FillRequiredFields(obj);

            obj.AsDynamic().TASK2JOBID = TestDecimal;
            obj.AsDynamic().EPSTASK2JOBJOBCODE = job.GetKey();
            obj.AsDynamic().EPSTASK2JOBTASKCODE = task.GetKey();
            obj.AsDynamic().TASK2JOBORDER = TestDecimal;
        }
        
        protected override void MakeSimpleChange(EpsTask2Job obj)
        {
            obj.AsDynamic().TASK2JOBORDER = TestDecimal + 1;
        }

        protected override void CheckSimpleChange(EpsTask2Job source, EpsTask2Job dest)
        {
            decimal sourceName = source.AsDynamic().TASK2JOBORDER;
            decimal destName = dest.AsDynamic().TASK2JOBORDER;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}