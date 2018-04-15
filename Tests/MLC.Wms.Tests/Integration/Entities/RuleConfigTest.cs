using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class RuleConfigTest : BaseEntityTest<RuleConfig>
    {
        protected override void FillRequiredFields(RuleConfig entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.MANDANTID = TstMandantId;
            obj.RULEID_R = RuleTest.ExistsItem1Id;
            obj.OPERATIONCODE_R = BillOperationTest.ExistsItem1Code;
            obj.RULECONFIGORDER = TestDecimal;
        }
    }
}