using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattCalcParamTest : BaseEntityTest<PattCalcParam>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCPARAMVALUE";
        }

        protected override void FillRequiredFields(PattCalcParam entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CalcDataSourceCode_r = PattCalcDataSourceTest.ExistsItem1Code;
            obj.TemplateParamsID_r = PattTParamsTest.ExistsItem1Id;
        }
    }
}