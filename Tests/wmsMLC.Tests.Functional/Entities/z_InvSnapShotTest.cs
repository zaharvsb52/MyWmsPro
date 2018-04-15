using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class InvSnapShotTest : BaseWMSObjectTest<InvSnapShot>
    {
        private readonly InvTest _invTest = new InvTest();
        private readonly InvGroupTest _invGroupTest = new InvGroupTest();
        private readonly SKUTest _skuTest = new SKUTest();
        private readonly ArtTest _artTest = new ArtTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _invTest, _invGroupTest, _skuTest, _artTest };
        }

        protected override void FillRequiredFields(InvSnapShot obj)
        {
            base.FillRequiredFields(obj);

            var inv = _invTest.CreateNew();
            _invGroupTest.TestString = TestString + "1";
            _invGroupTest.TestDecimal = TestDecimal + 1;
            _invGroupTest.TestGuid = new Guid("11111111111111111111111111111111");
            var invGroup = _invGroupTest.CreateNew();
            var sku = _skuTest.CreateNew();
            _artTest.TestString = TestString + "1";
            _artTest.TestDecimal = TestDecimal + 1;
            var art = _artTest.CreateNew();

            obj.AsDynamic().INVSSID = TestDecimal;
            obj.AsDynamic().INVID_R = inv.GetKey();
            obj.AsDynamic().INVGROUPID_R = invGroup.GetKey();
            obj.AsDynamic().ARTCODE_R = art.GetKey();
            obj.AsDynamic().SKUID_R = sku.GetKey();
            obj.AsDynamic().INVSSCOUNT = TestDecimal;
            obj.AsDynamic().INVSSCOUNT2SKU = TestDouble;
            obj.AsDynamic().INVSSPRODUCTINPUTDATE = DateTime.Now;
            obj.AsDynamic().INVSSPRODUCTDATE = DateTime.Now;
            obj.AsDynamic().INVSSEXPIRYDATE = DateTime.Now;
            obj.AsDynamic().INVSSBATCH = TestString;
            obj.AsDynamic().INVSSLOT = TestString;
            obj.AsDynamic().INVSSSERIALNUMBER = TestString;
            obj.AsDynamic().INVSSCOLOR = TestString;
            obj.AsDynamic().INVSSTONE = TestString;
            obj.AsDynamic().INVSSSIZE = TestString;
            obj.AsDynamic().INVSSBATCHCODE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(INVSSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(InvSnapShot obj)
        {
            obj.AsDynamic().INVSSTECODE = TestDecimal;
        }

        protected override void CheckSimpleChange(InvSnapShot source, InvSnapShot dest)
        {
            string sourceName = source.AsDynamic().INVSSTECODE;
            string destName = dest.AsDynamic().INVSSTECODE;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}