using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MinTest : BaseEntityTest<Min>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MINDesc";
        }

        protected override void FillRequiredFields(Min entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MINName = TestString;
            obj.PartnerID_r = TstMandantId;
        }
    }
}