using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class SKU2TTETest : BaseWMSObjectTest<SKU2TTE>
    {
        private  readonly TETypeTest _teTypeTest = new TETypeTest();
        private readonly SKUTest _skuTest = new SKUTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _teTypeTest, _skuTest };
        }

        protected override void FillRequiredFields(SKU2TTE obj)
        {
            base.FillRequiredFields(obj);

            var t = _teTypeTest.CreateNew();
            var s = _skuTest.CreateNew();

            obj.AsDynamic().SKU2TTEID = TestDecimal;
            obj.AsDynamic().SKU2TTETETYPECODE = t.GetKey();
            obj.AsDynamic().SKU2TTESKUID = s.GetKey();
            obj.AsDynamic().SKU2TTEQUANTITY = TestDecimal;
            obj.AsDynamic().SKU2TTEMAXWEIGHT = TestDecimal;
            obj.AsDynamic().SKU2TTELENGTH = TestDecimal;
            obj.AsDynamic().SKU2TTEWIDTH = TestDecimal;
            obj.AsDynamic().SKU2TTEHEIGHT = TestDecimal;
            obj.AsDynamic().SKU2TTEQUANTITYMAX = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SKU2TTEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(SKU2TTE obj)
        {
            obj.AsDynamic().SKU2TTEDEFAULT = true;
        }

        protected override void CheckSimpleChange(SKU2TTE source, SKU2TTE dest)
        {
            bool sourceName = source.AsDynamic().SKU2TTEDEFAULT;
            bool destName = dest.AsDynamic().SKU2TTEDEFAULT;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test(Description = DeleteByParentDesc)]
        public void DeleteByParentTest()
        {
            DeleteByParent<SKU>(TestDecimal, TestDecimal);
        }
    }
}