using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Entities
{
    [TestFixture]
    public class Worker2WarehouseTest : BaseEntityTest<Worker2Warehouse>
    {
        protected override void FillRequiredFields(Worker2Warehouse entity)
        {
            base.FillRequiredFields(entity);
            dynamic obj = entity.AsDynamic();

            obj.WORKER2WAREHOUSEWAREHOUSECODE = WarehouseTest.ExistsItem1Code;
            obj.Worker2WarehouseFrom = TestDateTime;
            obj.Worker2WarehouseTill = TestDateTime;
            
        }
    }
}