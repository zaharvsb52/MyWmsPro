using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ResTest : BaseWMSObjectTest<Res>
    {
        private readonly ProductTest _productTest = new ProductTest();
        private readonly OWBPosTest _owbPosTest = new OWBPosTest();
        private readonly MRTest _mrTest = new MRTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _productTest, _owbPosTest, _mrTest };
        }

        protected override void FillRequiredFields(Res obj)
        {
            base.FillRequiredFields(obj);

            _productTest.TestString = TestString + "1";
            var product = _productTest.CreateNew();
            _owbPosTest.TestString = TestString + "6";
            _owbPosTest.TestDecimal = TestDecimal + 6;
            var pos = _owbPosTest.CreateNew();
            var mr = _mrTest.CreateNew();

            obj.AsDynamic().RESID = TestDecimal;
            obj.AsDynamic().PRODUCTID_R = product.GetKey();
            obj.AsDynamic().OWBPOSID_R = pos.GetKey();
            obj.AsDynamic().MRCODE_R = mr.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(RESID = '{0}')", TestDecimal);
        }
    }
}