using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class Work2EntityTest : BaseWMSObjectTest<Work2Entity>
    {
        private readonly WorkTest _workTest = new WorkTest();
        private readonly WarehouseTest _warehouseTest = new WarehouseTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _workTest, _warehouseTest };
        }

        protected override void FillRequiredFields(Work2Entity obj)
        {
            base.FillRequiredFields(obj);

            var work = _workTest.CreateNew();

            obj.AsDynamic().WORK2ENTITYID = TestDecimal;
            obj.AsDynamic().WORK2ENTITYWORKID = work.GetKey();
            
            // Выбираем любую сущность 
            var warehouse = _warehouseTest.CreateNew();
            obj.AsDynamic().WORK2ENTITYENTITY = "WAREHOUSE";
            obj.AsDynamic().WORK2ENTITYKEY = warehouse.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WORK2ENTITYID = '{0}')", TestDecimal);
        }
    }
}