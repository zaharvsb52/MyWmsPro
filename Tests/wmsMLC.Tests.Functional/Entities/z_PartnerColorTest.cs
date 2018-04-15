using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PartnerColorTest : BaseWMSObjectTest<PartnerColor>
    {
        private readonly MandantTest _mandantTest = new MandantTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _mandantTest };
        }

        protected override void FillRequiredFields(PartnerColor obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PARTNERCOLORID = TestDecimal;
            obj.AsDynamic().PARTNERCOLORCODE = TestString;
            obj.AsDynamic().PARTNERCOLORNAME = TestString;
            obj.AsDynamic().MANDANTID = _mandantTest.CreateNew().GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PARTNERCOLORID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(PartnerColor obj)
        {
            obj.AsDynamic().PARTNERCOLORDESC = TestString;
        }

        protected override void CheckSimpleChange(PartnerColor source, PartnerColor dest)
        {
            string sourceName = source.AsDynamic().PARTNERCOLORDESC;
            string destName = dest.AsDynamic().PARTNERCOLORDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}