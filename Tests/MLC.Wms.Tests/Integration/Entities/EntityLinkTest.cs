using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class EntityLinkTest : BaseEntityTest<EntityLink>
    {
        //public const string ExistsItem1Code = "TST_ENTITYLINK_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "ENTITYLINKDESC";
        }

        protected override void FillRequiredFields(EntityLink entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.ENTITYLINKNAME = TestString;
            obj.ENTITYLINKFROM = TestString;
            obj.ENTITYLINKTO = TestString;
            obj.ENTITYLINKTYPE = TestString;
        }
    }
}