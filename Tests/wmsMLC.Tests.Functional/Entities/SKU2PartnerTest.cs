using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SKU2PartnerTest : BaseWMSObjectTest<SKU2Partner>
    {
        private readonly SKUTest _skuTest = new SKUTest();
        private readonly PartnerTest _partnerTest = new PartnerTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _partnerTest, _skuTest };
        }

        protected override void FillRequiredFields(SKU2Partner obj)
        {
            base.FillRequiredFields(obj);

            var p = _partnerTest.CreateNew();
            var s = _skuTest.CreateNew();

            obj.AsDynamic().SKU2PARTNERID = TestDecimal;
            obj.AsDynamic().SKU2PARTNERSKUID = s.GetKey();
            obj.AsDynamic().SKU2PARTNERPARTNERID = p.GetKey();
            obj.AsDynamic().SKU2PARTNERARTNAME = TestString;
            obj.AsDynamic().SKU2PARTNERARTDESC = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SKU2PARTNERID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(SKU2Partner obj)
        {
            obj.AsDynamic().SKU2PARTNERARTDESCEXT = TestString;
        }

        protected override void CheckSimpleChange(SKU2Partner source, SKU2Partner dest)
        {
            string sourceName = source.AsDynamic().SKU2PARTNERARTDESCEXT;
            string destName = dest.AsDynamic().SKU2PARTNERARTDESCEXT;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}