using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TransitDataTest : BaseEntityTest<TransitData>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TransitDataValue";
        }

        protected override void FillRequiredFields(TransitData entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TransitID_r = TransitTest.ExistsItem1Id;
            obj.TransitDataKey = TestString;
        }
    }
}