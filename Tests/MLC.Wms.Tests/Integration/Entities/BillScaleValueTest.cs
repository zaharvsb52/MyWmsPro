using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillScaleValueTest : BaseEntityTest<BillScaleValue>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SCALEVALUEVALUE";
        }

        protected override void FillRequiredFields(BillScaleValue entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SCALECODE_R = BillScaleTest.ExistsItem1Code;
            obj.SCALEVALUEFROM = TestString;
            obj.SCALEVALUETILL = TestString;
            obj.SCALEVALUEVALUE = TestString;
            obj.SCALEVALUETYPECODE_R = BillScaleValueTypeTest.ExistsItem1Code;
        }
    }
}