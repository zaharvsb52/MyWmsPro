using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MinSelectTest : BaseEntityTest<MinSelect>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MINSelectCritOwb";
        }

        protected override void FillRequiredFields(MinSelect entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MINId_r = MinTest.ExistsItem1Id;
            obj.MandantId = TstMandantId;
            obj.Priority = TestDecimal;
        }
    }
}