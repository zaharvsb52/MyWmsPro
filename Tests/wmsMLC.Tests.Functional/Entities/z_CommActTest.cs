using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class CommActTest : BaseWMSObjectTest<CommAct>
    {
        private readonly InvTest _invTest = new InvTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invTest };
        }

        protected override void FillRequiredFields(CommAct obj)
        {
            base.FillRequiredFields(obj);

            var inv = _invTest.CreateNew();

            obj.AsDynamic().COMMACTID = TestDecimal;
            obj.AsDynamic().COMMACTNAME = TestString;
            obj.AsDynamic().INVID_R = inv.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(COMMACTID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(CommAct obj)
        {
            obj.AsDynamic().COMMACTDESC = TestString;
        }

        protected override void CheckSimpleChange(CommAct source, CommAct dest)
        {
            string sourceName = source.AsDynamic().COMMACTDESC;
            string destName = dest.AsDynamic().COMMACTDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}