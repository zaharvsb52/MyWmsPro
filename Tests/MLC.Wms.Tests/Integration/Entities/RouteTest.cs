using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class RouteTest : BaseEntityTest<Route>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "RouteHostRef";
        }

        protected override void FillRequiredFields(Route entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MandantID = TstMandantId;
            obj.RouteNumber = TestDecimal;
            obj.RouteDate = TestDateTime;
        }
    }
}