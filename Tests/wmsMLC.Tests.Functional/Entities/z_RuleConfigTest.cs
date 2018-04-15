using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RuleConfigTest : BaseWMSObjectTest<RuleConfig>
    {
        private readonly RuleTest _ruletest = new RuleTest(); 

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[]
                {
                  _ruletest
                };
        }

        protected override void FillRequiredFields(RuleConfig obj)
        {
            base.FillRequiredFields(obj);

            _ruletest.TestDecimal = TestDecimal;
            var rule = _ruletest.CreateNew();

            obj.AsDynamic().RULECONFIGID = TestDecimal;
            obj.AsDynamic().RULEID_R = rule.GetKey();
            obj.AsDynamic().OPERATIONCODE_R = "OP_UPDATE";
            obj.AsDynamic().RULECONFIGORDER = TestDecimal;
            obj.AsDynamic().MANDANTID = 1;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RULECONFIGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(RuleConfig obj)
        {
            obj.AsDynamic().OPERATIONCODE_R = "OP_DELETE";
        }

        protected override void CheckSimpleChange(RuleConfig source, RuleConfig dest)
        {
            string sourceName = source.AsDynamic().OPERATIONCODE_R;
            string destName = dest.AsDynamic().OPERATIONCODE_R;
            sourceName.ShouldBeEquivalentTo(destName);
        }


    }
}