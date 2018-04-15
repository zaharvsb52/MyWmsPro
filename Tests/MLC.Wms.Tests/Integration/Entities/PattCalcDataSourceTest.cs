using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattCalcDataSourceTest : BaseEntityTest<PattCalcDataSource>
    {
        public const string ExistsItem1Code = "TST_PATTCALCDATASOURCE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCDATASOURCEDESC";
        }

        protected override void FillRequiredFields(PattCalcDataSource entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CALCDATASOURCECODE = TestString;
            obj.CALCDATASOURCENAME = TestString;
            obj.TEMPLATEDATASOURCECODE_R = PattTDataSourceTest.ExistsItem1Code;
        }
    }
}