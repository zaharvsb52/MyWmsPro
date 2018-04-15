using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RuleParamTest : BaseWMSObjectTest<RuleParam>
    {
        private readonly RuleTest _ruletest = new RuleTest(); 

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[]
                {
                  _ruletest
                };
        }

        protected override void FillRequiredFields(RuleParam obj)
        {
            base.FillRequiredFields(obj);


            _ruletest.TestDecimal = TestDecimal;
            var rule = _ruletest.CreateNew();

            obj.AsDynamic().RULEPARAMID = TestDecimal;
            obj.AsDynamic().RULEID_R = rule.GetKey();
            obj.AsDynamic().RULEPARAMNAME = TestString;
            obj.AsDynamic().RULEPARAMDATATYPE = TestDecimal;
            obj.AsDynamic().RULEPARAMMUSTSET = false;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RULEPARAMID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(RuleParam obj)
        {
            obj.AsDynamic().RULEPARAMNAME = TestString;
        }

        protected override void CheckSimpleChange(RuleParam source, RuleParam dest)
        {
            string sourceName = source.AsDynamic().RULEPARAMNAME;
            string destName = dest.AsDynamic().RULEPARAMNAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}