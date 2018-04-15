using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class NewVehicleTest : NewBaseWMSObjectTest<Vehicle>
    {
        protected override void FillRequaredFields(Vehicle obj)
        {
            base.FillRequaredFields(obj);

            obj.AsDynamic().VEHICLEID = TestDecimal;
            obj.AsDynamic().VEHICLERN = TestString;
            obj.AsDynamic().CARTYPEID_R = TestDecimalAnchor;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(VEHICLEID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Vehicle obj)
        {
            obj.AsDynamic().VEHICLERN = TestString;
        }

        protected override void CheckSimpleChange(Vehicle source, Vehicle dest)
        {
            string sourceName = source.AsDynamic().VEHICLERN;
            string destName = dest.AsDynamic().VEHICLERN;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}