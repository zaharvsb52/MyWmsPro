using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class IWBPosTest : BaseEntityTest<IWBPos>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "IWBPosGTD";
        }

        protected override void FillRequiredFields(IWBPos entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.IWBID_r = IWBTest.ExistsItem1Id;
            obj.IWBPosNumber = TestDecimal;
            obj.SKUID_r = SKUTest.ExistsItem1Id;
            obj.IWBPosCount = TestDecimal;
            obj.IWBPosCount2SKU = TestDouble;
            obj.StatusCode_r = TestString;
            obj.IWBPosManual = TestBool;
            obj.IWBPosOwner = TstMandantId;
        }
    }
}