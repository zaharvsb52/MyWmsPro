using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    class EpsTaskTest : BaseWMSObjectTest<EpsTask>
    {
        public const bool TestBool = false;
        private readonly SysEnumTest _sysEnumTest = new SysEnumTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _sysEnumTest };
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(TASKCODE = '{0}')", TestString);
        }

        protected override void FillRequiredFields(EpsTask obj)
        {
            base.FillRequiredFields(obj);

            var sysEnum = _sysEnumTest.CreateNew();
            
            obj.AsDynamic().TASKCODE = TestString;
            obj.AsDynamic().TASKNAME = TestString;
            obj.AsDynamic().TASKLOCKED = TestBool;
            obj.AsDynamic().TASKTYPE = sysEnum.GetKey();
        }

        protected override void MakeSimpleChange(EpsTask obj)
        {
            obj.AsDynamic().TASKDESC = TestString;
        }

        protected override void CheckSimpleChange(EpsTask source, EpsTask dest)
        {
            string sourceName = source.AsDynamic().TASKDESC;
            string destName = dest.AsDynamic().TASKDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
