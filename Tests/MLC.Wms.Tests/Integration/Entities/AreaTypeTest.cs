using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class AreaTypeTest : BaseEntityTest<AreaType>
    {
        public const string ExistsItem1Code = "TST_AREATYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "AREATYPEDESC";
        }

        protected override void FillRequiredFields(AreaType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.AREATYPENAME = TestString;
        }
    }
}