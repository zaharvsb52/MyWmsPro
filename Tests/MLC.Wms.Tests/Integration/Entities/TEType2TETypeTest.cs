using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class TEType2TETypeTest : BaseEntityTest<TEType2TEType>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "TEType2TETypeDesc";
        }

        protected override void FillRequiredFields(TEType2TEType entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.TEType2TETypeSlave = TETypeTest.ExistsItem1Code;
            obj.TEType2TETypeMaster = TETypeTest.ExistsItem2Code;
            obj.TEType2TETypeCapacity = TestDecimal;
        }
    }
}