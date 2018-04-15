using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillSpecialFuncEntityTest : BaseEntityTest<BillSpecialFuncEntity>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "SPECIALFUNCTIONENTITYBODY";
        }

        protected override void FillRequiredFields(BillSpecialFuncEntity entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.SPECIALFUNCTIONCODE_R = BillSpecialFunctionTest.ExistsItem1Code;
            obj.SPECIALFUNCENTITYOBJECTENTITY = "BILLSPECIALFUNCENTITY";
        }
    }
}