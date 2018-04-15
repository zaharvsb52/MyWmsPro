using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MgRouteSelectTest : BaseEntityTest<MgRouteSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MgRouteSelectRegion";
        }

        protected override void FillRequiredFields(MgRouteSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.Priority = TestDecimal;
            obj.MgRouteID_r = MgRouteTest.ExistsItem1Id;
        }
    }
}