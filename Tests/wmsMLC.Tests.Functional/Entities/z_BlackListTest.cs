using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BlackListTest : BaseWMSObjectTest<BlackList>
    {
        private readonly WorkerTest _workerTest = new WorkerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _workerTest };
        }

        protected override void FillRequiredFields(BlackList obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().BLACKLISTID = TestDecimal;
            obj.AsDynamic().WORKERID_R = _workerTest.CreateNew().GetKey();
            obj.AsDynamic().BLACKLISTDATE = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(BLACKLISTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(BlackList obj)
        {
            obj.AsDynamic().BLACKLISTDESK = TestString;
        }

        protected override void CheckSimpleChange(BlackList source, BlackList dest)
        {
            string sourceName = source.AsDynamic().BLACKLISTDESK;
            string destName = dest.AsDynamic().BLACKLISTDESK;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}