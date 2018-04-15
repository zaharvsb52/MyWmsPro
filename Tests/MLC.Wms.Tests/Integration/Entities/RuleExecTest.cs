using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class RuleExecTest : BaseEntityTest<RuleExec>
    {
        public const int ExistsItem1Id = -1;
        protected override void FillRequiredFields(RuleExec entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.RuleID_r = RuleTest.ExistsItem1Id;
        }
    }
}