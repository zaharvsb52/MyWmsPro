using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class PattTFieldTest : BaseEntityTest<PattTField>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEMPLATEFIELDNAME";
        }

        protected override void FillRequiredFields(PattTField entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEMPLATEFIELDSECTIONID_R = PattTFieldSectionTest.ExistsItem1Id;
            obj.TEMPLATEFIELDNAME = TestString;
            obj.TEMPLATEFIELDALIAS = TestString;
            obj.TEMPLATEFIELDDATATYPE = TestDecimal;
        }
    }
}