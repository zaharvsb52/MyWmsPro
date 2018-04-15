using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class InvTaskStepTest : BaseEntityTest<InvTaskStep>
    {
        public const decimal ExistsItem1Id = -1;

        protected override void FillRequiredFields(InvTaskStep entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.InvTaskStepNumber = TestDecimal;
            obj.InvTaskGroupID_r = InvTaskGroupTest.ExistsItem1Id;
        }
    }
}