using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvGroupTest : BaseEntityTest<InvGroup>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "INVGROUPNAME";
        }

        protected override void FillRequiredFields(InvGroup entity)
        {
            base.FillRequiredFields(entity);
            var obj = entity.AsDynamic();

            obj.INVID_R = InvTest.ExistsItem1Id;
            obj.INVGROUPNAME = TestString;
            obj.INVGROUPFILTER = TestString;
        }
    }
}
