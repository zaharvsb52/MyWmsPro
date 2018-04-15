using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class BillCalcConfigDetailTest : BaseEntityTest<BillCalcConfigDetail>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "CALCCONFIGDETAILFIELDSOURCE";
        }

        protected override void FillRequiredFields(BillCalcConfigDetail entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.CALCCONFIGID_R = BillCalcConfigTest.ExistsItem1Id;
            obj.CALCCONFIGDETAILDESTINATION = TestString;
        }
    }
}