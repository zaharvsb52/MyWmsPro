using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class UserEnumTest : BaseEntityTest<UserEnum>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "UserEnumDesc";
        }

        protected override void FillRequiredFields(UserEnum entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.UserEnumGroup = TestString;
            obj.UserEnumKey = TestString;
        }
    }
}