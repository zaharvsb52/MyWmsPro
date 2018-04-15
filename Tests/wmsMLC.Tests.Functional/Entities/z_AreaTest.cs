using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class AreaTest : BaseWMSObjectTest<Area>
    {
        private readonly AreaTypeTest _areaTypeTest = new AreaTypeTest();
        private readonly WarehouseTest _warehouseTest = new WarehouseTest();

        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[]
                {
                   _areaTypeTest,_warehouseTest
                };
        }

        protected override void FillRequiredFields(Area obj)
        {
            base.FillRequiredFields(obj);

            _areaTypeTest.TestString = TestString;
            var areaType = _areaTypeTest.CreateNew();

            _warehouseTest.TestString = TestString;
            var warehouseTest = _warehouseTest.CreateNew();
           
            obj.AsDynamic().AREACODE = TestString;
            obj.AsDynamic().AREANAME = TestString;
            obj.AsDynamic().AREATYPECODE_R = areaType.GetKey();
            obj.AsDynamic().WAREHOUSECODE_R = warehouseTest.GetKey();
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(AREACODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Area obj)
        {
            obj.AsDynamic().AREADESC = TestString;
        }

        protected override void CheckSimpleChange(Area source, Area dest)
        {
            string sourceName = source.AsDynamic().AREADESC;
            string destName = dest.AsDynamic().AREADESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }

        [Test, Ignore("Нет истории")]
        public override void ManagerGetHistoryTest()
        {
        }
    }
}