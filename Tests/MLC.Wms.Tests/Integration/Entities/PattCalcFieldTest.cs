using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattCalcFieldTest : BaseEntityTest<PattCalcField>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCFIELDFUNCTIONPARAM1";
        }

        protected override void FillRequiredFields(PattCalcField entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CALCDATASOURCECODE_R = PattCalcDataSourceTest.ExistsItem1Code;
            obj.TEMPLATEFIELDSECTIONID_R = PattTFieldSectionTest.ExistsItem1Id;
            obj.SPECIALFUNCTIONCODE_R = BillSpecialFunctionTest.ExistsItem1Code;
            obj.CALCFIELDDATATYPE = TestDecimal;
        }
    }
}