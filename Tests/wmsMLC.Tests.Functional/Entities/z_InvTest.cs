using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvTest : BaseWMSObjectTest<Inv>
    {
        private readonly MITest _miTest = new MITest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _miTest };
        }
        
        protected override void FillRequiredFields(Inv obj)
        {
            base.FillRequiredFields(obj);

            var mi = _miTest.CreateNew();

            obj.AsDynamic().INVID = TestDecimal;
            obj.AsDynamic().INVNAME = TestString;
            obj.AsDynamic().MICODE_R = mi.GetKey();
            obj.AsDynamic().MANDANTID = 1;
            obj.AsDynamic().INVDATEBEGIN = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Inv obj)
        {
            obj.AsDynamic().INVDESC = TestString;
        }

        protected override void CheckSimpleChange(Inv source, Inv dest)
        {
            string sourceName = source.AsDynamic().INVDESC;
            string destName = dest.AsDynamic().INVDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}