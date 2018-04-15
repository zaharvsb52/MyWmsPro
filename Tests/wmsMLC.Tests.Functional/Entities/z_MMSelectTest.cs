using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class MMSelectTest : BaseWMSObjectTest<MMSelect>
    {
        private readonly MMTest _mmTest = new MMTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mmTest };
        }

        protected override void FillRequiredFields(MMSelect obj)
        {
            base.FillRequiredFields(obj);

            var mm = _mmTest.CreateNew();

            obj.AsDynamic().MMSELECTID = TestDecimal;
            obj.AsDynamic().MMCODE_R = mm.GetKey();
            obj.AsDynamic().PRIORITY = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(MMSELECTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(MMSelect obj)
        {
            obj.AsDynamic().MMSELECTTECOMPLETEMIN = TestDecimal;
        }

        protected override void CheckSimpleChange(MMSelect source, MMSelect dest)
        {
            decimal sourceValue = source.AsDynamic().MMSELECTTECOMPLETEMIN;
            decimal destValue = dest.AsDynamic().MMSELECTTECOMPLETEMIN;
            sourceValue.ShouldBeEquivalentTo(destValue);
        }
    }
}