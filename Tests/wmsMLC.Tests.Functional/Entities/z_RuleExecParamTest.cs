using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RuleExecParamTest : BaseWMSObjectTest<RuleExecParam>
    {
        private readonly RuleExecTest _ruleExectest = new RuleExecTest();
        private readonly RuleParamTest _ruleParamtest = new RuleParamTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _ruleExectest,_ruleParamtest };
        }

        protected override void FillRequiredFields(RuleExecParam obj)
        {
            base.FillRequiredFields(obj);

            _ruleExectest.TestDecimal = TestDecimal+1;
            _ruleParamtest.TestDecimal = TestDecimal+2;
            var ruleexec = _ruleExectest.CreateNew();
            var ruleexecparam = _ruleParamtest.CreateNew();

            obj.AsDynamic().RULEEXECPARAMID = TestDecimal;
            obj.AsDynamic().RULEEXECID_R = ruleexec.GetKey();
            obj.AsDynamic().RULEPARAMID_R = ruleexecparam.GetKey();
            
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RULEEXECPARAMID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(RuleExecParam obj)
        {
            obj.AsDynamic().RULEEXECPARAMVALUE = TestString;
        }

        protected override void CheckSimpleChange(RuleExecParam source, RuleExecParam dest)
        {
            string sourceName = source.AsDynamic().RULEEXECPARAMVALUE;
            string destName = dest.AsDynamic().RULEEXECPARAMVALUE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}