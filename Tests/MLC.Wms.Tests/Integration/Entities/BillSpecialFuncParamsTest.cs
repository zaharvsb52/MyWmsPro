using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillSpecialFuncParamsTest : BaseEntityTest<BillSpecialFuncParams>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SPECIALFUNCTIONPARAMSNAME";
        }

        protected override void FillRequiredFields(BillSpecialFuncParams entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SPECIALFUNCTIONCODE_R = BillSpecialFunctionTest.ExistsItem1Code;
            obj.SPECIALFUNCTIONPARAMSNAME = TestString;
            obj.SPECIALFUNCTIONPARAMSORDER = TestDecimal;
        }
    }
}