using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MMTest : BaseEntityTest<MM>
    {
        public const string ExistsItem1Code = "TST_MM_1";

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MMDesc";
        }

        protected override void FillRequiredFields(MM entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MMName = TestString;
        }
    }
}