using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ArtPriceTest : BaseWMSObjectTest<ArtPrice>
    {
        private readonly SKUTest _skuTest = new SKUTest();

        public ArtPriceTest()
        {
            _skuTest.TestString = TestString;
        }

        protected override void FillRequiredFields(ArtPrice obj)
        {
            base.FillRequiredFields(obj);

            var s = _skuTest.CreateNew();

            obj.AsDynamic().ARTPRICEID = TestDecimal;
            obj.AsDynamic().SKUID_R = s.GetKey();
            obj.AsDynamic().ARTPRICEVALUE = TestDouble;
            obj.AsDynamic().ARTPRICEVAT = TestDouble;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(ARTPRICEID = {0})", TestDecimal);
        }

        //protected override void MakeSimpleChange(ArtPrice obj)
        //{
        //}

        //protected override void CheckSimpleChange(ArtPrice source, ArtPrice dest)
        //{
        //}

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _skuTest.ClearForSelf();
        }
    }
}