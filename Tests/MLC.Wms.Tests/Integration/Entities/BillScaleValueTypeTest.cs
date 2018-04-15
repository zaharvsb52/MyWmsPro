using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillScaleValueTypeTest : BaseEntityTest<BillScaleValueType>
    {
        public const string ExistsItem1Code = "TST_BILLSCALEVALUETYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SCALEVALUETYPENAME";
        }

        protected override void FillRequiredFields(BillScaleValueType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SCALEVALUETYPENAME = TestString;
        }
    }
}