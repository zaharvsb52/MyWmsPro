using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class PLPosTest : BaseWMSObjectTest<PLPos>
    {
        private readonly PLTest _plTest = new PLTest();
        private readonly PlaceTest _placeTest = new PlaceTest();
        private readonly SKUTest _skuTest = new SKUTest();
        private readonly TETest _teTest = new TETest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _plTest, _placeTest, _skuTest, _teTest };
        }

        protected override void FillRequiredFields(PLPos obj)
        {
            base.FillRequiredFields(obj);

            var place = _placeTest.CreateNew();
            _skuTest.TestDecimal = TestDecimal + 1;
            _skuTest.TestString = TestString + "1";
            var sku = _skuTest.CreateNew();
            var pl = _plTest.CreateNew();
            _teTest.TestDecimal = TestDecimal + 2;
            _teTest.TestString = TestString + "2";
            var te = _teTest.CreateNew();

            obj.AsDynamic().PLPOSID = TestDecimal;
            obj.AsDynamic().PLID_R = pl.GetKey();
            obj.AsDynamic().TECODE_R = te.GetKey();
            obj.AsDynamic().PLACECODE_R = place.GetKey();
            obj.AsDynamic().PLPOSSORT = TestDecimal;
            obj.AsDynamic().SKUID_r = sku.GetKey();
            obj.AsDynamic().PLPOSCOUNTSKUPLAN = TestDecimal;
            obj.AsDynamic().PLPOSCOUNTSKUFACT = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PLPOSID = '{0}')", TestDecimal);
        }
    }
}