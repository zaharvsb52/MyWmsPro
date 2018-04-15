using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class ObjectConfigTest : BaseEntityTest<ObjectConfig>
    {
        public const string ExistsItem1Code = "TST_OBJECTCONFIG_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "OBJECTCONFIGDESC";
        }

        protected override void FillRequiredFields(ObjectConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.OBJECTCONFIGNAME = TestString;
        }
    }
}