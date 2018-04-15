using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PartnerColorTest : BaseEntityTest<PartnerColor>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "PARTNERCOLORDESC";
        }

        protected override void FillRequiredFields(PartnerColor entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.PARTNERCOLORCODE = TestString;
            obj.MANDANTID = TstMandantId;
        }
    }
}