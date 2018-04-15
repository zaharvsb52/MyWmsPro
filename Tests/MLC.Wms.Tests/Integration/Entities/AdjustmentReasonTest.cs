using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class AdjustmentReasonTest : BaseEntityTest<AdjustmentReason>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ADJUSTMENTREASONDESC";
        }

        protected override void FillRequiredFields(AdjustmentReason entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ADJUSTMENTREASONCODE = TestString;
            obj.MANDANTID = TstMandantId;
        }
    }
}