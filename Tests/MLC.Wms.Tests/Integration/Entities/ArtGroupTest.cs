using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ArtGroupTest : BaseEntityTest<ArtGroup>
    {
        public const string ExistsItem1Code = "TST_ARTGROUP_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ARTGROUPDESC";
        }

        protected override void FillRequiredFields(ArtGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ARTGROUPNAME = TestString;
        }
    }
}