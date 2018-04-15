using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TruckTypeTest : BaseEntityTest<TruckType>
    {
        public const string ExistsItem1Code = "TST_TRUCKTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TruckTypeDesc";
        }

        protected override void FillRequiredFields(TruckType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TruckTypeName = TestString;
            obj.TruckTypeWeightMax = TestDecimal;
            obj.TruckTypePickCount = TestDecimal;
        }
    }
}