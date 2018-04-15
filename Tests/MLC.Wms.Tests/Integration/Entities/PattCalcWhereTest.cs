using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattCalcWhereTest : BaseEntityTest<PattCalcWhere>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCWHEREPARAM1";
        }

        protected override void FillRequiredFields(PattCalcWhere entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CalcDataSourceCode_r = PattCalcDataSourceTest.ExistsItem1Code;
            obj.TemplateWhereSectionID_r = PattTWhereSectionTest.ExistsItem1Id;
        }
    }
}