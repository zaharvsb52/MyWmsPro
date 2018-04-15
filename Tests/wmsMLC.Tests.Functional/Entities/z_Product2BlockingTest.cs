using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Product2BlockingTest : BaseWMSObjectTest<Product2Blocking>
    {
        private readonly ProductTest _productTest = new ProductTest();
        private readonly ProductBlockingTest _productBlockingTest = new ProductBlockingTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _productTest, _productBlockingTest };
        }

        protected override void FillRequiredFields(Product2Blocking obj)
        {
            base.FillRequiredFields(obj);

            var product = _productTest.CreateNew();
            var block = _productBlockingTest.CreateNew(bl =>
            {
                bl.AsDynamic().BLOCKINGFORPRODUCT = true;
            });

            obj.AsDynamic().PRODUCT2BLOCKINGID = TestDecimal;
            obj.AsDynamic().PRODUCT2BLOCKINGPRODUCTID = product.GetKey();
            obj.AsDynamic().PRODUCT2BLOCKINGBLOCKINGCODE = block.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PRODUCT2BLOCKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Product2Blocking obj)
        {
            obj.AsDynamic().PRODUCT2BLOCKINGDESC = TestString;
        }

        protected override void CheckSimpleChange(Product2Blocking source, Product2Blocking dest)
        {
            string sourceName = source.AsDynamic().PRODUCT2BLOCKINGDESC;
            string destName = dest.AsDynamic().PRODUCT2BLOCKINGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}