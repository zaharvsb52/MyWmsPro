using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Art2GroupTest : BaseEntityTest<Art2Group>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Art2Group.Art2GroupPriorityPropertyName;
        }

        protected override void FillRequiredFields(Art2Group entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ART2GROUPPRIORITY = TestDecimal;
            obj.ART2GROUPARTCODE = ArtTest.ExistsItem1Code;
            obj.ART2GROUPARTGROUPCODE = ArtGroupTest.ExistsItem1Code;
        }
    }
}