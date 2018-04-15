using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class FactoryTest : BaseEntityTest<Factory>
    {
        public const decimal ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = Factory.FactoryNamePropertyName;
        }

        protected override void FillRequiredFields(Factory entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.FACTORYCODE = TestString.Substring(0, 3);
            obj.FACTORYNAME = TestString;
            obj.PARTNERID_R = TstMandantId;
        }
    }
}