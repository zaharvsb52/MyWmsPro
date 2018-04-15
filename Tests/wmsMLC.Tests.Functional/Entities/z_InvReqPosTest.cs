using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvReqPosTest : BaseWMSObjectTest<InvReqPos>
    {
        private readonly InvReqTest _invReqTest = new InvReqTest();
        private readonly ArtTest _artTest = new ArtTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invReqTest, _artTest };
        }

        protected override void FillRequiredFields(InvReqPos obj)
        {
            base.FillRequiredFields(obj);

            var invReq = _invReqTest.CreateNew();
            var art = _artTest.CreateNew();

            obj.AsDynamic().INVREQPOSID = TestDecimal;
            obj.AsDynamic().INVREQID_R = invReq.GetKey();
            obj.AsDynamic().ARTCODE_R = art.GetKey();
            obj.AsDynamic().INVREQPOSARTNAME = TestString;
            obj.AsDynamic().INVREQPOSMEASURE = TestString;
            obj.AsDynamic().INVREQPOSNUMBER = TestDecimal;
            obj.AsDynamic().INVREQPOSHOSTREF = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVREQPOSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(InvReqPos obj)
        {
            obj.AsDynamic().INVREQPOSCOUNT = TestDecimal;
        }

        protected override void CheckSimpleChange(InvReqPos source, InvReqPos dest)
        {
            decimal sourceName = source.AsDynamic().INVREQPOSCOUNT;
            decimal destName = dest.AsDynamic().INVREQPOSCOUNT;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}