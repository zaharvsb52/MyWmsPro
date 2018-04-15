using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvReqTest : BaseWMSObjectTest<InvReq>
    {
        private readonly InvTest _invTest = new InvTest();
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invTest, _mandantTest };
        }

        protected override void FillRequiredFields(InvReq obj)
        {
            base.FillRequiredFields(obj);

            var inv = _invTest.CreateNew();
            _mandantTest.TestString = TestString + "1";
            _mandantTest.TestDecimal = TestDecimal + 1;
            var mandant = _mandantTest.CreateNew();

            obj.AsDynamic().INVREQID = TestDecimal;
            obj.AsDynamic().MANDANTID = mandant.GetKey();
            obj.AsDynamic().INVID_R = inv.GetKey();
            obj.AsDynamic().INVREQNAME = TestString;
            obj.AsDynamic().INVREQDATEPLAN = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVREQID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(InvReq obj)
        {
            obj.AsDynamic().INVREQHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(InvReq source, InvReq dest)
        {
            string sourceName = source.AsDynamic().INVREQHOSTREF;
            string destName = dest.AsDynamic().INVREQHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}