using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTDataSourceTest : BaseEntityTest<PattTDataSource>
    {
        public const string ExistsItem1Code = "TST_PATTTDATASOURCE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEDATASOURCEDESC";
        }

        protected override void FillRequiredFields(PattTDataSource entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEDATASOURCECODE = TestString;
            obj.TEMPLATEDATASOURCENAME = TestString;
            obj.TEMPLATEDATASOURCETYPE = TestString;
        }
    }
}