using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class WarehouseTest : BaseWMSObjectTest<Warehouse>
    {
        protected override void FillRequiredFields(Warehouse obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().WAREHOUSECODE = TestString;
            obj.AsDynamic().WAREHOUSENAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(WAREHOUSECODE = '{0}')", TestString);
        }

        protected override void MakeSimpleChange(Warehouse obj)
        {
            obj.AsDynamic().WAREHOUSEDESC = TestString;
        }

        protected override void CheckSimpleChange(Warehouse source, Warehouse dest)
        {
            string sourceName = source.AsDynamic().WAREHOUSEDESC;
            string destName = dest.AsDynamic().WAREHOUSEDESC;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}