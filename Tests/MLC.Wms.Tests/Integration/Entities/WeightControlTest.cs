using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class WeightControlTest : BaseEntityTest<WeightControl>
    { 
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "WeightControlArtGroup";
        }

        protected override void FillRequiredFields(WeightControl entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.Priority = TestDecimal;
            obj.MandantID = TstMandantId;
            obj.WeightControlDev = TestDouble;
        }
    }
}