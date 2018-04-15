using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class KitPosTest : BaseWMSObjectTest<KitPos>
    {
        private readonly KitTest _kitTest = new KitTest();
        private readonly SKUTest _skuTest = new SKUTest();

        public KitPosTest()
        {
            _kitTest.TestString = TestString;
            _skuTest.TestString = TestString;
        }

        protected override void FillRequiredFields(KitPos obj)
        {
            base.FillRequiredFields(obj);

            var kit = _kitTest.CreateNew();
            var sku = _skuTest.CreateNew();

            obj.AsDynamic().KITPOSID = TestDecimal;
            obj.AsDynamic().KITCODE_R = kit.GetKey();
            obj.AsDynamic().SKUID_R = sku.GetKey();
            obj.AsDynamic().KITPOSCOUNT = TestDecimal;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(KITPOSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(KitPos obj)
        {
            obj.AsDynamic().KITPOSPRIORITY = TestDecimal;
        }
        
        protected override void CheckSimpleChange(KitPos source, KitPos dest)
        {
            decimal sourceName = source.AsDynamic().KITPOSPRIORITY;
            decimal destName = dest.AsDynamic().KITPOSPRIORITY;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _kitTest.ClearForSelf();
            _skuTest.ClearForSelf();
        }
    }
}