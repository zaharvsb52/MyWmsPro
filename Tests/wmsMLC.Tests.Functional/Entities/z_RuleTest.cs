using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class RuleTest : BaseWMSObjectTest<Rule>
    {
        protected override void FillRequiredFields(Rule obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().RULEID = TestDecimal;
            obj.AsDynamic().RULENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RULEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Rule obj)
        {
            obj.AsDynamic().RULENAME = TestString;
        }

        protected override void CheckSimpleChange(Rule source, Rule dest)
        {
            string sourceName = source.AsDynamic().RULENAME;
            string destName = dest.AsDynamic().RULENAME;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}