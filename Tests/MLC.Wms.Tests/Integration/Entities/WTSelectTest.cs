using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WTSelectTest : BaseEntityTest<WTSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WTSelectDoc";
        }

        protected override void FillRequiredFields(WTSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PartnerId_r = TstMandantId;
            obj.OperationCode_r = BillOperationTest.ExistsItem1Code;
        }
    }
}