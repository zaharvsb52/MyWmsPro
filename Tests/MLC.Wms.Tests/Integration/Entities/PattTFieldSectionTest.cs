using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTFieldSectionTest : BaseEntityTest<PattTFieldSection>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEFIELDSECTIONDESC";
        }

        protected override void FillRequiredFields(PattTFieldSection entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEDATASOURCECODE_R = PattTDataSourceTest.ExistsItem1Code;
            obj.TEMPLATEFIELDSECTIONNAME = TestString;
            obj.TEMPLATEFIELDSECTIONCODE = TestString;
            obj.TEMPLATEFIELDSECTIONRESULT = TestBool;
            obj.TEMPLATEFIELDSECTIONDETERM = TestBool;
        }
    }
}