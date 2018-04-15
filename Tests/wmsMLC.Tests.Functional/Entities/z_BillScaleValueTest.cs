using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class BillScaleValueTest : BaseWMSObjectTest<BillScaleValue>
    {
        private readonly BillScaleTest _billScaleTest = new BillScaleTest();
        private readonly BillScaleValueTypeTest _billScaleValueTypeTest = new BillScaleValueTypeTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _billScaleTest, _billScaleValueTypeTest };
        }

        protected override void FillRequiredFields(BillScaleValue obj)
        {
            base.FillRequiredFields(obj);

            var scale = _billScaleTest.CreateNew();
            var type = _billScaleValueTypeTest.CreateNew();

            obj.AsDynamic().SCALEVALUEID = TestDecimal;
            obj.AsDynamic().SCALECODE_R = scale.GetKey();
            obj.AsDynamic().SCALEVALUEFROM = TestString;
            obj.AsDynamic().SCALEVALUETILL = TestString;
            obj.AsDynamic().SCALEVALUEVALUE = TestString;
            obj.AsDynamic().SCALEVALUETYPECODE_R = type.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(SCALEVALUEID = '{0}')", TestDecimal);
        }
    }
}