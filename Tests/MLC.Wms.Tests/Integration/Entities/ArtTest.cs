using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ArtTest : BaseEntityTest<Art>
    {
        public const string ExistsItem1Code = "TST_ART_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Art.ARTDESCPropertyName;
        }

        protected override void FillRequiredFields(Art entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ARTNAME = TestString;
            obj.MANDANTID = TstMandantId;
            obj.ARTABCD = "A";
        }
    }
}