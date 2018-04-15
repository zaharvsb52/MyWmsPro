using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Worker2WarehouseTest : BaseWMSObjectTest<Worker2Warehouse>
    {
        private readonly WorkerGroupTest _workerGroupTest = new WorkerGroupTest();
        private readonly WorkerTest _workerTest = new WorkerTest();
        private readonly WarehouseTest _warehouseTest = new WarehouseTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _workerGroupTest, _workerTest, _warehouseTest };
        }

        protected override void FillRequiredFields(Worker2Warehouse obj)
        {
            base.FillRequiredFields(obj);

            var group = _workerGroupTest.CreateNew();
            var worker = _workerTest.CreateNew();
            var house = _warehouseTest.CreateNew();

            obj.AsDynamic().WORKER2WAREHOUSEID = TestDecimal;
            obj.AsDynamic().WORKER2WAREHOUSEWORKERGROUPID = group.GetKey();
            obj.AsDynamic().WORKER2WAREHOUSEWAREHOUSECODE = house.GetKey();
            obj.AsDynamic().WORKER2WAREHOUSEFROM = DateTime.Now;
            obj.AsDynamic().WORKER2WAREHOUSETILL = DateTime.Now;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORKER2WAREHOUSEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Worker2Warehouse obj)
        {
            obj.AsDynamic().WORKER2WAREHOUSEWORKERID = TestDecimal;
        }

        protected override void CheckSimpleChange(Worker2Warehouse source, Worker2Warehouse dest)
        {
            decimal sourceName = source.AsDynamic().WORKER2WAREHOUSEWORKERID;
            decimal destName = dest.AsDynamic().WORKER2WAREHOUSEWORKERID;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}