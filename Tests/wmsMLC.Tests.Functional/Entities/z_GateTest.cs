using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class GateTest : BaseWMSObjectTest<Gate>
    {
        private readonly WarehouseTest _warehouseTest = new WarehouseTest();
        protected override IEnumerable<BaseWMSTest> GetDependencyTests()
        {
            return new BaseWMSTest[] { _warehouseTest };
        }

        protected override void FillRequiredFields(Gate obj)
        {
            base.FillRequiredFields(obj);

            var w = _warehouseTest.CreateNew();

            obj.AsDynamic().GATECODE = TestString;
            obj.AsDynamic().GATENAME = TestString;
            obj.AsDynamic().WAREHOUSECODE_R = w.GetKey();
            obj.AsDynamic().GATENUMBER = "1";
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(GateCode = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Gate obj)
        {
            obj.AsDynamic().GATENAME = TestString;
        }

        protected override void CheckSimpleChange(Gate source, Gate dest)
        {
            string sourceName = source.AsDynamic().GATENUMBER;
            string destName = dest.AsDynamic().GATENUMBER;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}