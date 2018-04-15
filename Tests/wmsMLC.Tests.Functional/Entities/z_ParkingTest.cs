using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class ParkingTest : BaseWMSObjectTest<Parking>
    {
        protected override void FillRequiredFields(Parking obj)
        {
            base.FillRequiredFields(obj);

            obj.AsDynamic().PARKINGID = TestDecimal;
            obj.AsDynamic().PARKINGNUMBER = TestString;
            obj.AsDynamic().PARKINGNAME = TestString;
        }

        protected override string GetCheckFilter()
        {
            return string.Format("(PARKINGID = '{0}')", TestDecimal);
        }

        protected override void MakeSimpleChange(Parking obj)
        {
            obj.AsDynamic().PARKINGAREA = TestString;
        }

        protected override void CheckSimpleChange(Parking source, Parking dest)
        {
            string sourceName = source.AsDynamic().PARKINGAREA;
            string destName = dest.AsDynamic().PARKINGAREA;
            sourceName.ShouldBeEquivalentTo(destName);
        }
    }
}