using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MRUseTest : BaseEntityTest<MRUse>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MRUseStrategyValue";
        }

        protected override void FillRequiredFields(MRUse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MRCode_r = MRTest.ExistsItem1Code;
            obj.MRUseStrategyType = TestString;
            obj.MRUseStrategy = TestString;
            obj.MRUseOrder = TestDecimal;
        }
    }
}