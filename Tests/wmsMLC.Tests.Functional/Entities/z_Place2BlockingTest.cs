using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Place2BlockingTest : BaseWMSObjectTest<Place2Blocking>
    {
        private readonly PlaceTest _placeTest = new PlaceTest();
        private readonly ProductBlockingTest _productBlockingTest = new ProductBlockingTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _placeTest, _productBlockingTest };
        }

        protected override void FillRequiredFields(Place2Blocking obj)
        {
            base.FillRequiredFields(obj);

            var p = _placeTest.CreateNew();
            var pb = _productBlockingTest.CreateNew(t => t.AsDynamic().BLOCKINGFORPLACE = true);

            obj.AsDynamic().PLACE2BLOCKINGID = TestDecimal;
            obj.AsDynamic().PLACE2BLOCKINGPLACECODE = p.GetKey();
            obj.AsDynamic().PLACE2BLOCKINGBLOCKINGCODE = pb.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PLACE2BLOCKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Place2Blocking obj)
        {
            obj.AsDynamic().PLACE2BLOCKINGDESC = TestString;
        }

        protected override void CheckSimpleChange(Place2Blocking source, Place2Blocking dest)
        {
            string sourceName = source.AsDynamic().PLACE2BLOCKINGDESC;
            string destName = dest.AsDynamic().PLACE2BLOCKINGDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}