using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business.Objects;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class VehicleTest : BaseWMSObjectTest<Vehicle>
    {
        private readonly CarTypeTest _carTypeTest = new CarTypeTest();

        public VehicleTest()
        {
            _carTypeTest.TestString = TestString;
        }

        protected override void FillRequiredFields(Vehicle obj)
        {
            base.FillRequiredFields(obj);

            var ct = _carTypeTest.CreateNew();

            obj.AsDynamic().VEHICLEID = TestDecimal;
            obj.AsDynamic().VEHICLERN = TestString;
            obj.AsDynamic().CARTYPEID_R = ct.GetKey();
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

        public override void ClearForSelf()
        {
            base.ClearForSelf();
            _carTypeTest.ClearForSelf();
        }
    }
}
