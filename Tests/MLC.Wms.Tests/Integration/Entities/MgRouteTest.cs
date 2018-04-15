using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MgRouteTest : BaseEntityTest<MgRoute>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MgRouteDesc";
        }

        protected override void FillRequiredFields(MgRoute entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MandantID = TstMandantId;
            obj.MgRouteName = TestString;
            obj.MgRouteNumber = TestDecimal;
            obj.MgRouteCreateRoute = TestBool;
            
        }
    }
}