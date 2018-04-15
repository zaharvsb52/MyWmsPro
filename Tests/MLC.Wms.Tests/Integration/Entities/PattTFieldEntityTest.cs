using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTFieldEntityTest : BaseEntityTest<PattTFieldEntity>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEFIELDENTITYALIASENTITY";
        }

        protected override void FillRequiredFields(PattTFieldEntity entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEFIELDSECTIONID_R = PattTFieldSectionTest.ExistsItem1Id;
            obj.TFIELDENTITYOBJECTENTITY = TestString;
        }
    }
}