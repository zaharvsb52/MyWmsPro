using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Worker2GroupTest : BaseEntityTest<Worker2Group>
    {
        protected override void FillRequiredFields(Worker2Group entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORKER2GROUPWORKERGROUPID = WorkerGroupTest.ExistsItem1Id;
            obj.WORKER2GROUPWORKERID = WorkerTest.ExistsItem1Id;
            
        }
    }
}