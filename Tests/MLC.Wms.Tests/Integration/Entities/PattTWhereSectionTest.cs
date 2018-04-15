using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTWhereSectionTest : BaseEntityTest<PattTWhereSection>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEWHERESECTIONDESC";
        }

        protected override void FillRequiredFields(PattTWhereSection entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEDATASOURCECODE_R = PattTDataSourceTest.ExistsItem1Code;
            obj.TEMPLATEWHERESECTIONCODE = TestString;
            obj.TEMPLATEWHERESECTIONNAME = TestString;
        }
    }
}