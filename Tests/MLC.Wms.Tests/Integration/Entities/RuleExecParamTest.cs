using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class RuleExecParamTest : BaseEntityTest<RuleExecParam>
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            SimpleChangePropertyName = "RuleExecParamNote";
        }

        protected override void FillRequiredFields(RuleExecParam entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.RuleExecID_r = RuleExecTest.ExistsItem1Id;
            obj.RuleParamID_r = RuleParamTest.ExistsItem1Id;
            obj.RuleExecParamValue = TestDecimal;
        }
    }
}