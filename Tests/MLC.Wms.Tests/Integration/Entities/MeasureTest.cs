using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MeasureTest : BaseEntityTest<Measure>
    {
        public const string ExistsItem1Code = "TST_MEASURE_1";
        public const string ExistsItem2Code = "TST_MEASURE_2";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Measure.MeasureNamePropertyName;
        }

        protected override void FillRequiredFields(Measure entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MEASUREFACTOR = TestDouble;
            obj.MEASURETYPECODE_R = MeasureTypeTest.ExistsItem1Code;
        }
    }
}