using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class RuleParamTest : BaseEntityTest<RuleParam>
    {
        public const int ExistsItem1Id = -1;

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "RuleParamDesc";
        }

        protected override void FillRequiredFields(RuleParam entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.RuleID_r = RuleTest.ExistsItem1Id;
            obj.RuleParamName = TestString;
            obj.RuleParamDataType = TestDecimal;
            obj.RuleParamMustSet = TestBool;

        }
    }
}