using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RuleExecTest : BaseWMSObjectTest<RuleExec>
    {
        private readonly RuleTest _ruletest = new RuleTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[]
                {
                  _ruletest
                };
        }


        protected override void FillRequiredFields(RuleExec obj)
        {
            base.FillRequiredFields(obj);

            _ruletest.TestDecimal = TestDecimal;
            var rule = _ruletest.CreateNew();

            obj.AsDynamic().RULEEXECID = TestDecimal;
            obj.AsDynamic().RULEID_R = rule.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RULEEXECID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(RuleExec obj)
        {
            obj.AsDynamic().RULEID_R = TestDecimal;
        }

        protected override void CheckSimpleChange(RuleExec source, RuleExec dest)
        {
            decimal sourceName = source.AsDynamic().RULEID_R;
            decimal destName = dest.AsDynamic().RULEID_R;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}