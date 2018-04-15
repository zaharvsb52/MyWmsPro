using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvTaskGroupTest : BaseEntityTest<InvTaskGroup>
    {
        public const decimal ExistsItem1Id = -1;

        protected override void FillRequiredFields(InvTaskGroup entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.InvGroupID_r = InvGroupTest.ExistsItem1Id;
            obj.InvTaskGroupNumber = TestDecimal;
        }
    }
}