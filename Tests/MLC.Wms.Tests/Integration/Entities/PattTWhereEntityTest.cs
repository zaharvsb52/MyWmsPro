using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTWhereEntityTest : BaseEntityTest<PattTWhereEntity>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEWHEREENTITYALIASENTITY";
        }

        protected override void FillRequiredFields(PattTWhereEntity entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEWHERESECTIONID_R = PattTWhereSectionTest.ExistsItem1Id;
            obj.TWHEREENTITYOBJECTENTITY = TestString;
        }
    }
}