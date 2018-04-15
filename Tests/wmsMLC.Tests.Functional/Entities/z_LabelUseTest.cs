using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class LabelUseTest : BaseWMSObjectTest<LabelUse>
    {
        private readonly LabelTest _labelTest = new LabelTest();
        //private readonly SKUTest _skuTest = new SKUTest();
        //private readonly ArtTest _artTest = new ArtTest();
        //private readonly ArtGroupTest _artGroupTest = new ArtGroupTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            //return new BaseWMSTest[] {_labelTest, _skuTest, _artTest, _artGroupTest};
            return new BaseWMSTest[] {_labelTest};
        }

        protected override void FillRequiredFields(LabelUse obj)
        {
            base.FillRequiredFields(obj);

            var label = _labelTest.CreateNew();
            //var sku = _skuTest.CreateNew();
            //var art = _artTest.CreateNew();
            //var artGroup = _artGroupTest.CreateNew();

            obj.SetProperty(obj.GetPrimaryKeyPropertyName(), TestDecimal);
            obj.AsDynamic().LABELCODE_R = label.GetKey();
            //obj.AsDynamic().SKUID_R = sku.GetKey();
            //obj.AsDynamic().ARTCODE_R = art.GetKey();
            //obj.AsDynamic().ARTGROUPCODE_R = artGroup.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(LABELUSEID = {0})", TestDecimal);
        }

        //protected override void MakeSimpleChange(LabelUse obj)
        //{
        //    var label = _labelTest.CreateNew();
        //    obj.AsDynamic().LABELCODE_R = label.GetKey();
        //}

        protected override void CheckSimpleChange(LabelUse source, LabelUse dest)
        {
            string sourceName = source.AsDynamic().LABELCODE_R;
            string destName = dest.AsDynamic().LABELCODE_R;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}
