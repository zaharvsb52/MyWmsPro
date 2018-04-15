using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class KitTest : BaseEntityTest<Kit>
    {
        public const string ExistsItem1Code = "TST_KIT_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Kit.KitPriorityInPropertyName;
        }

        protected override void FillRequiredFields(Kit entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.KITTYPECODE_R = KitTypeTest.ExistsItem1Code;
        }
    }
}