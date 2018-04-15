using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class OWBPosTest : BaseWMSObjectTest<OWBPos>
    {
        private readonly OWBTest _owbTest = new OWBTest();
        private readonly SKUTest _skuTest = new SKUTest();
        private readonly QlfTest _qlfTest = new QlfTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _owbTest, _skuTest, _qlfTest };
        }

        protected override void FillRequiredFields(OWBPos obj)
        {
            base.FillRequiredFields(obj);

            _owbTest.TestDecimal = TestDecimal + 1;
            _owbTest.TestString = TestString + "1";
            var owb = _owbTest.CreateNew();
            _skuTest.TestDecimal = TestDecimal + 2;
            _skuTest.TestString = TestString + "2";
            var sku = _skuTest.CreateNew();
            _qlfTest.TestDecimal = TestDecimal + 3;
            _qlfTest.TestString = TestString + "3";
            var qlf = _qlfTest.CreateNew();

            obj.AsDynamic().OWBPOSID = TestDecimal;
            obj.AsDynamic().OWBID_R = owb.GetKey();
            obj.AsDynamic().OWBPOSNUMBER = TestDecimal;
            obj.AsDynamic().SKUID_R = sku.GetKey();
            obj.AsDynamic().OWBPOSCOUNT = TestDecimal;
            obj.AsDynamic().OWBPOSCOLOR = TestString;
            obj.AsDynamic().OWBPOSTONE = TestString;
            obj.AsDynamic().OWBPOSSIZE = TestString;
            obj.AsDynamic().OWBPOSBATCH = TestString;
            obj.AsDynamic().OWBPOSEXPIRYDATE = DateTime.Now;
            obj.AsDynamic().OWBPOSPRODUCTDATE = DateTime.Now;
            obj.AsDynamic().OWBPOSSERIALNUMBER = TestString;
            //obj.AsDynamic().OWBPOSFACTORY = TestDecimal;
            obj.AsDynamic().QLFCODE_R = qlf.GetKey();
            obj.AsDynamic().OWBPOSPRICEVALUE = TestDouble;
            obj.AsDynamic().OWBPOSARTNAME = TestString;
            obj.AsDynamic().OWBPOSMEASURE = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(OWBPOSID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(OWBPos obj)
        {
            obj.AsDynamic().OWBPOSHOSTREF = TestString;
        }

        protected override void CheckSimpleChange(OWBPos source, OWBPos dest)
        {
            string sourceName = source.AsDynamic().OWBPOSHOSTREF;
            string destName = dest.AsDynamic().OWBPOSHOSTREF;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}