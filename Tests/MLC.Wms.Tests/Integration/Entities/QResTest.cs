using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class QResTest : BaseEntityTest<QRes>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OWBReservType_r";
        }

        protected override void FillRequiredFields(QRes entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MandantID = TstMandantId;
            obj.OWBID_r = OwbTest.ExistsItem1Code;
            obj.OWBPriority_r = TestDecimal;
            obj.OWBProductNeed_r = TestString;
            obj.StatusCode_r = TestString;
        }
    }
}