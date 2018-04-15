using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class KitTypeTest : BaseEntityTest<KitType>
    {
        public const string ExistsItem1Code = "TST_KITTYPE_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = KitType.KitTypeDescPropertyName;
        }

        protected override void FillRequiredFields(KitType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.KITTYPENAME = TestString;
        }
    }
}