using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MMUseTest : BaseWMSObjectTest<MMUse>
    {
        private readonly MMTest _mmTest = new MMTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] {_mmTest};
        }

        protected override void FillRequiredFields(MMUse obj)
        {
            base.FillRequiredFields(obj);

            var mm = _mmTest.CreateNew();

            obj.AsDynamic().MMUSEID = TestDecimal;
            obj.AsDynamic().MMCODE_R = mm.GetKey();
            obj.AsDynamic().MMUSESTRATEGY = TestString;
            obj.AsDynamic().MMUSEPRIORITY = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MMUSEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MMUse obj)
        {
            obj.AsDynamic().MMUSESTRATEGYVALUE = TestString;
        }

        protected override void CheckSimpleChange(MMUse source, MMUse dest)
        {
            string sourceValue = source.AsDynamic().MMUSESTRATEGYVALUE;
            string destValue = dest.AsDynamic().MMUSESTRATEGYVALUE;
            sourceValue.ShouldBeEquivalentTo(destValue);
        }
    }
}