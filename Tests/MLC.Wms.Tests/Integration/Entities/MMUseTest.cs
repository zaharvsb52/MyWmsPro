using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class MMUseTest : BaseEntityTest<MMUse>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "MMUseStrategyValue";
        }

        protected override void FillRequiredFields(MMUse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MMCode_r = MMTest.ExistsItem1Code;
            obj.MMUsePriority = TestDecimal;
            obj.MMUseStrategy = TestString;
        }
    }
}