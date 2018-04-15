using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class W2E2WorkingTest : BaseEntityTest<W2E2Working>
    {
        protected override void FillRequiredFields(W2E2Working entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.W2E2WORKINGWORKINGID = WorkingTest.ExistsItem1Id;
            obj.W2E2WORKINGWORK2ENTITYID = Work2EntityTest.ExistsItem1Id;
        }
    }
}